'use strict';
const fs = require('fs');
const os = require('os');
const path = require('path');
const tsPath = process.env.PKC_TYPESCRIPT_PATH || require.resolve('typescript');
const ts = require(tsPath);
const { createResolver } = require('../../src/Pkc.Frontend/AngularUrlExpressionResolver.cjs');
const root = fs.mkdtempSync(path.join(os.tmpdir(),'pkc-url-regression-'));
try {
  fs.mkdirSync(path.join(root,'env'),{recursive:true});
  fs.writeFileSync(path.join(root,'env','environment.ts'), "export const environment = { apiUrl: '/api/' };\n");
  fs.writeFileSync(path.join(root,'service.ts'), `
    import { environment } from './env/environment';
    export class OrderService {
      private baseUrl = environment.apiUrl;
      markPacked(id: string) { return this.http.put(this.baseUrl + 'order/' + id + '/pack', {}); }
      template(id: string) { const segment = 'order'; return this.http.put(\`\${this.baseUrl}\${segment}/\${id}/pack\`, {}); }
      unsafe(id: number) { return this.http.put('/api/order/' + (id + 1) + '/pack', {}); }
    }
  `);
  const resolver = createResolver(ts, root);
  const sf = resolver.loadSourceFile(path.join(root,'service.ts'));
  function methodName(node) { for(let c=node.parent;c;c=c.parent) if(ts.isMethodDeclaration(c)) return c.name.getText(sf); return null; }
  function arg(name) { let result=null; function visit(n){if(ts.isCallExpression(n)&&ts.isPropertyAccessExpression(n.expression)&&n.expression.name.text==='put'&&methodName(n)===name)result=n.arguments[0];ts.forEachChild(n,visit);}visit(sf);return result; }
  const resolved = resolver.resolveStringExpression(arg('markPacked'));
  if (resolved !== '/api/order/{param}/pack') throw new Error(`concat resolution: ${resolved}`);
  if (resolver.normalizeRouteKey(resolved) !== '/api/order/{param}/pack') throw new Error('route normalization');
  if (resolver.resolveStringExpression(arg('template')) !== '/api/order/{param}/pack') throw new Error('template/local const');
  if (resolver.resolveStringExpression(arg('unsafe')) !== null) throw new Error('numeric arithmetic must fail closed');

  fs.writeFileSync(path.join(root,'duplicate.ts'), `class D { run(id:string){ const prefix='/api/a/'; const prefix='/api/b/'; return this.http.put(prefix+id,{}); } }`);
  const duplicate = resolver.loadSourceFile(path.join(root,'duplicate.ts'));
  let duplicateArg=null; function visitDuplicate(n){if(ts.isCallExpression(n)&&ts.isPropertyAccessExpression(n.expression)&&n.expression.name.text==='put')duplicateArg=n.arguments[0];ts.forEachChild(n,visitDuplicate);}visitDuplicate(duplicate);
  if (resolver.resolveStringExpression(duplicateArg) !== null) throw new Error('ambiguous local const must fail closed');

  fs.writeFileSync(path.join(root,'mutable.ts'), `class S { private baseUrl='/api/'; set(){this.baseUrl='/other/'} run(id:string){return this.http.put(this.baseUrl+'order/'+id+'/pack',{})} }`);
  const mutable = resolver.loadSourceFile(path.join(root,'mutable.ts'));
  let mutableArg=null; function visit(n){if(ts.isCallExpression(n)&&ts.isPropertyAccessExpression(n.expression)&&n.expression.name.text==='put')mutableArg=n.arguments[0];ts.forEachChild(n,visit);}visit(mutable);
  if (resolver.resolveStringExpression(mutableArg) !== null) throw new Error('mutable class property must fail closed');

  if (resolver.normalizeRouteKey('https://example.test/api/order/{param}/pack?x=1') !== '/api/order/{param}/pack') throw new Error('absolute URL normalization');
  console.log('angular URL expression regression PASS');
} finally { fs.rmSync(root,{recursive:true,force:true}); }
