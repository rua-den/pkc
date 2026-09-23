'use strict';

const fs = require('fs');
const path = require('path');

function createResolver(ts, repositoryRoot) {
  const root = path.resolve(repositoryRoot);
  const sourceFileCache = new Map();

  function isUnderRoot(fullPath) {
    const relative = path.relative(root, path.resolve(fullPath));
    return relative === '' ||
      (relative !== '..' &&
       !relative.startsWith(`..${path.sep}`) &&
       !path.isAbsolute(relative));
  }

  function loadSourceFile(filePath) {
    const fullPath = path.resolve(filePath);
    if (!isUnderRoot(fullPath) || !fs.existsSync(fullPath)) return null;
    if (sourceFileCache.has(fullPath)) return sourceFileCache.get(fullPath);

    const text = fs.readFileSync(fullPath, 'utf8');
    const sourceFile = ts.createSourceFile(
      fullPath,
      text,
      ts.ScriptTarget.Latest,
      true,
      fullPath.endsWith('.tsx') ? ts.ScriptKind.TSX : ts.ScriptKind.TS);
    sourceFileCache.set(fullPath, sourceFile);
    return sourceFile;
  }

  function propertyName(node) {
    if (!node) return null;
    if (ts.isIdentifier(node) || ts.isStringLiteral(node) || ts.isNumericLiteral(node)) return node.text;
    return node.getText();
  }

  function nearestMethod(node) {
    for (let current = node?.parent; current; current = current.parent) {
      if (ts.isMethodDeclaration(current)) return current;
    }
    return null;
  }

  function nearestClass(node) {
    for (let current = node?.parent; current; current = current.parent) {
      if (ts.isClassDeclaration(current)) return current;
    }
    return null;
  }

  function methodParameter(identifier, useNode) {
    const method = nearestMethod(useNode);
    if (!method) return null;

    const parameter = method.parameters.find(candidate =>
      ts.isIdentifier(candidate.name) && candidate.name.text === identifier);
    if (!parameter) return null;

    const type = parameter.type?.getText(parameter.getSourceFile()) ?? '';
    return { text: '{param}', stringLike: type === 'string' };
  }

  function constInitializerFromStatements(statements, beforePosition, identifier) {
    const matches = [];
    for (const statement of statements) {
      if (statement.getStart() >= beforePosition) break;
      if (!ts.isVariableStatement(statement) ||
          (statement.declarationList.flags & ts.NodeFlags.Const) === 0) continue;

      for (const declaration of statement.declarationList.declarations) {
        if (ts.isIdentifier(declaration.name) &&
            declaration.name.text === identifier &&
            declaration.initializer) {
          matches.push(declaration.initializer);
        }
      }
    }
    return matches.length === 1 ? matches[0] : null;
  }

  function visibleConstInitializer(identifier, useNode) {
    let current = useNode;
    while (current?.parent) {
      const parent = current.parent;
      if (ts.isBlock(parent) || ts.isSourceFile(parent)) {
        const initializer = constInitializerFromStatements(
          parent.statements,
          current.getStart(),
          identifier);
        if (initializer) return initializer;
      }
      current = parent;
    }
    return null;
  }

  function isAssignmentOperator(kind) {
    return kind >= ts.SyntaxKind.FirstAssignment && kind <= ts.SyntaxKind.LastAssignment;
  }

  function assignsThisProperty(node, propertyNameValue) {
    if (ts.isBinaryExpression(node) && isAssignmentOperator(node.operatorToken.kind)) {
      return ts.isPropertyAccessExpression(node.left) &&
        node.left.expression.kind === ts.SyntaxKind.ThisKeyword &&
        node.left.name.text === propertyNameValue;
    }

    if (ts.isPrefixUnaryExpression(node) || ts.isPostfixUnaryExpression(node)) {
      const operand = node.operand;
      return ts.isPropertyAccessExpression(operand) &&
        operand.expression.kind === ts.SyntaxKind.ThisKeyword &&
        operand.name.text === propertyNameValue;
    }

    return false;
  }

  function classPropertyInitializer(classDeclaration, propertyNameValue) {
    const matches = classDeclaration.members.filter(member =>
      ts.isPropertyDeclaration(member) &&
      propertyName(member.name) === propertyNameValue &&
      member.initializer);
    if (matches.length !== 1) return null;

    let mutated = false;
    function visit(node) {
      if (mutated) return;
      if (assignsThisProperty(node, propertyNameValue)) {
        mutated = true;
        return;
      }
      ts.forEachChild(node, visit);
    }

    for (const member of classDeclaration.members) {
      if (member !== matches[0]) visit(member);
    }

    return mutated ? null : matches[0].initializer;
  }

  function resolveRelativeModule(sourceFile, moduleSpecifier) {
    if (!moduleSpecifier.startsWith('.')) return null;

    const base = path.resolve(path.dirname(sourceFile.fileName), moduleSpecifier);
    const candidates = [base, `${base}.ts`, `${base}.tsx`, path.join(base, 'index.ts')];
    for (const candidate of candidates) {
      if (isUnderRoot(candidate) && fs.existsSync(candidate)) return loadSourceFile(candidate);
    }
    return null;
  }

  function importedBinding(identifier, sourceFile) {
    const matches = [];
    for (const statement of sourceFile.statements) {
      if (!ts.isImportDeclaration(statement) ||
          !ts.isStringLiteral(statement.moduleSpecifier) ||
          !statement.importClause?.namedBindings ||
          !ts.isNamedImports(statement.importClause.namedBindings)) continue;

      for (const element of statement.importClause.namedBindings.elements) {
        if (element.name.text !== identifier) continue;
        const importedSource = resolveRelativeModule(sourceFile, statement.moduleSpecifier.text);
        if (!importedSource) continue;
        matches.push({
          sourceFile: importedSource,
          exportedName: element.propertyName?.text ?? element.name.text
        });
      }
    }
    return matches.length === 1 ? matches[0] : null;
  }

  function hasExportModifier(statement) {
    const modifiers = ts.getModifiers ? ts.getModifiers(statement) : statement.modifiers;
    return (modifiers ?? []).some(modifier => modifier.kind === ts.SyntaxKind.ExportKeyword);
  }

  function exportedConstInitializer(sourceFile, identifier) {
    const matches = [];
    for (const statement of sourceFile.statements) {
      if (!ts.isVariableStatement(statement) ||
          !hasExportModifier(statement) ||
          (statement.declarationList.flags & ts.NodeFlags.Const) === 0) continue;

      for (const declaration of statement.declarationList.declarations) {
        if (ts.isIdentifier(declaration.name) &&
            declaration.name.text === identifier &&
            declaration.initializer) {
          matches.push(declaration.initializer);
        }
      }
    }
    return matches.length === 1 ? matches[0] : null;
  }

  function unwrap(node) {
    let current = node;
    while (current &&
           (ts.isAsExpression(current) ||
            ts.isTypeAssertionExpression(current) ||
            ts.isParenthesizedExpression(current))) {
      current = current.expression;
    }
    return current;
  }

  function objectPropertyInitializer(objectExpression, propertyNameValue) {
    const object = unwrap(objectExpression);
    if (!object || !ts.isObjectLiteralExpression(object)) return null;

    const matches = object.properties.filter(property =>
      ts.isPropertyAssignment(property) && propertyName(property.name) === propertyNameValue);
    return matches.length === 1 ? matches[0].initializer : null;
  }

  function hasObjectPropertyWrite(sourceFile, objectName, propertyNameValue) {
    let found = false;
    function isTarget(node) {
      return ts.isPropertyAccessExpression(node) &&
        ts.isIdentifier(node.expression) &&
        node.expression.text === objectName &&
        node.name.text === propertyNameValue;
    }
    function visit(node) {
      if (found) return;
      if (ts.isBinaryExpression(node) &&
          isAssignmentOperator(node.operatorToken.kind) &&
          isTarget(node.left)) {
        found = true;
        return;
      }
      ts.forEachChild(node, visit);
    }
    visit(sourceFile);
    return found;
  }

  function evaluate(node, useNode = node, stack = new Set()) {
    if (!node) return null;
    const current = unwrap(node);
    if (!current) return null;

    const sourceFile = current.getSourceFile();
    const key = `${sourceFile.fileName}:${current.pos}:${current.end}`;
    if (stack.has(key) || stack.size > 24) return null;
    const next = new Set(stack);
    next.add(key);

    if (ts.isStringLiteral(current) || ts.isNoSubstitutionTemplateLiteral(current)) {
      return { text: current.text, stringLike: true };
    }
    if (ts.isNumericLiteral(current)) {
      return { text: current.text, stringLike: false };
    }

    if (ts.isBinaryExpression(current) && current.operatorToken.kind === ts.SyntaxKind.PlusToken) {
      const left = evaluate(current.left, useNode, next);
      const right = evaluate(current.right, useNode, next);
      if (!left || !right || (!left.stringLike && !right.stringLike)) return null;
      return { text: left.text + right.text, stringLike: true };
    }

    if (ts.isTemplateExpression(current)) {
      let text = current.head.text;
      for (const span of current.templateSpans) {
        const value = evaluate(span.expression, useNode, next);
        if (!value) return null;
        text += value.text + span.literal.text;
      }
      return { text, stringLike: true };
    }

    if (ts.isIdentifier(current)) {
      const parameter = methodParameter(current.text, useNode);
      if (parameter) return parameter;

      const local = visibleConstInitializer(current.text, useNode);
      if (local) return evaluate(local, local, next);

      const imported = importedBinding(current.text, sourceFile);
      if (!imported) return null;
      const initializer = exportedConstInitializer(imported.sourceFile, imported.exportedName);
      return initializer ? evaluate(initializer, initializer, next) : null;
    }

    if (ts.isPropertyAccessExpression(current)) {
      const name = current.name.text;
      if (current.expression.kind === ts.SyntaxKind.ThisKeyword) {
        const classDeclaration = nearestClass(useNode);
        const initializer = classDeclaration
          ? classPropertyInitializer(classDeclaration, name)
          : null;
        return initializer ? evaluate(initializer, initializer, next) : null;
      }

      if (ts.isIdentifier(current.expression)) {
        const objectName = current.expression.text;
        const localObject = visibleConstInitializer(objectName, useNode);
        if (localObject) {
          const property = objectPropertyInitializer(localObject, name);
          return property ? evaluate(property, property, next) : null;
        }

        const imported = importedBinding(objectName, sourceFile);
        if (!imported) return null;
        const initializer = exportedConstInitializer(imported.sourceFile, imported.exportedName);
        if (!initializer || hasObjectPropertyWrite(imported.sourceFile, imported.exportedName, name)) {
          return null;
        }
        const property = objectPropertyInitializer(initializer, name);
        return property ? evaluate(property, property, next) : null;
      }
    }

    return null;
  }

  function normalizeRouteKey(value) {
    let route = value.trim();
    const absolute = /^[A-Za-z][A-Za-z0-9+.-]*:\/\/[^/]+(?<path>\/[^?#]*)?(?:[?#].*)?$/.exec(route);
    if (absolute) route = absolute.groups?.path || '/';
    else route = route.split('?')[0].split('#')[0];

    route = route
      .replace(/\$\{[^}]+\}/g, '{param}')
      .replace(/\{[^}/]+\}|:[A-Za-z0-9_]+/g, '{param}')
      .replace(/\/{2,}/g, '/');
    if (!route.startsWith('/')) route = '/' + route;
    return route.replace(/\/$/, '').toLowerCase();
  }

  return {
    loadSourceFile,
    resolveStringExpression(node) {
      return evaluate(node)?.text ?? null;
    },
    normalizeRouteKey
  };
}

module.exports = { createResolver };
