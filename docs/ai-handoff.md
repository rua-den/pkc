# AI Handoff Contract

PKC exists so a Product Owner can give compiled product/system knowledge to a capable AI without asking that AI to re-read the source repository.

This document defines how generated knowledge is handed to an AI. The contract is vendor-neutral.

## Principle

The canonical compiled knowledge is the structured `knowledge/` directory. Transport formats exist only to make that directory easy to hand to different AI products.

```text
source repository
  ↓
PKC
  ↓
knowledge/                  canonical portable knowledge
  ├─ AI_INSTRUCTIONS.md     AI entrypoint / usage contract
  ├─ index.md               capability map and knowledge boundary
  ├─ features/**/*.md       product/system capability summaries
  └─ workflows/**/*.md      operation details and trace evidence

PKC_KNOWLEDGE.md            convenience single-file bundle
PKC_KNOWLEDGE.zip           archive transport of knowledge/
```

Do not make ZIP parsing a requirement for PKC knowledge consumption. Archive support differs across AI products.

## Recommended PO experience

### Path A — single-file upload

For an AI product that accepts ordinary document/file upload, the simplest user experience should be:

```text
1. Run: pkc build <repository>
2. Upload PKC_KNOWLEDGE.md to the AI
3. Ask the product/system question
```

The bundle must contain the same knowledge hierarchy and authority/unknown boundaries as the canonical folder.

This path is convenient for small and medium knowledge packs. Very large systems may eventually require sharding/retrieval rather than one giant context file.

### Path B — structured multi-file knowledge

If the AI/workspace supports uploading or indexing multiple files, provide the generated `knowledge/` directory contents.

The AI must read `knowledge/AI_INSTRUCTIONS.md` first, then `knowledge/index.md`, then feature/workflow files as needed.

This preserves retrieval granularity and avoids forcing every question through one large document.

### Path C — ZIP transport

`PKC_KNOWLEDGE.zip` is a convenience archive containing the canonical `knowledge/` pack.

Use it when the destination can inspect archives, or as a simple artifact for storage/sharing. Do not require a PO to manually understand the internal directory layout before using PKC.

If the destination cannot inspect ZIP content, use the single-file bundle or the extracted multi-file pack instead.

## AI instructions contract

`knowledge/AI_INSTRUCTIONS.md` must tell the receiving AI to:

1. start with `index.md` for orientation;
2. use feature files for product/system capability questions;
3. open workflow files for detailed validations, failures, state changes, side effects, integrations and source evidence;
4. treat `authority: code-observed` as implementation evidence, not approved business intent;
5. respect `Important unknowns` and never invent missing UI/runtime/delivery/product-intent evidence;
6. distinguish production behavior from conditional/development-only behavior;
7. prefer explicit unknown over unsupported inference;
8. cite the relevant knowledge file/path when explaining an important claim when practical.

The instructions must not contain repository-specific behavior. They describe how to consume any PKC knowledge pack.

## Bundle contract

`PKC_KNOWLEDGE.md` is a deterministic transport rendering of the canonical `knowledge/` files.

It must:

- begin with the AI instructions and index;
- preserve file boundaries with explicit source path headings;
- include feature files before workflow files;
- preserve authority, unknowns and evidence sections;
- not silently omit a file that exists in the canonical pack;
- be regeneratable from the canonical knowledge pack.

The canonical individual Markdown files remain the source of truth for the portable knowledge representation; the bundle is a convenience view.

## Archive contract

`PKC_KNOWLEDGE.zip` contains the canonical `knowledge/` directory contents using stable relative paths.

It must not include source code, build artifacts or `.pkc/facts.json` by default. A PO should be able to share the knowledge artifact without unintentionally sharing the source repository.

## What PKC does not do yet

V0.4.x does not require a live vendor-specific connection.

Future delivery options may include:

```text
pkc serve / MCP
AI connector/plugin
workspace synchronization
knowledge API/retrieval service
```

Those are delivery adapters. They should be added only after the portable knowledge contract is proven trustworthy. They must not replace or weaken the vendor-neutral knowledge pack.

## Acceptance question

The handoff succeeds when a PO can take the generated artifact, give it to an AI with minimal instructions, and the AI can answer important product/system questions correctly without access to the source repository.
