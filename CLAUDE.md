# PSMS Project Instructions

## Codebase Patterns

When writing code for this project, follow the patterns documented in `.claude/skills/psms-patterns.md`. This covers:
- **Backend:** Entity, AppService, DTO, Mapper, Interface, ExceptionCodes, and Permission patterns (ASP.NET Boilerplate)
- **Frontend:** Provider (Context + useReducer + redux-actions), Form Modal (Zod + Ant Design), Page Content (EnterpriseTable), TypeScript interfaces, and API patterns (Next.js 16 + Ant Design v6)

## Frontend Form Components

When building any frontend component that contains forms, always use **Zod** for schema validation. Define Zod schemas for form inputs and use them to validate data before submission.
