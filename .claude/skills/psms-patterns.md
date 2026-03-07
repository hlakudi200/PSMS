# PSMS Codebase Patterns Skill

Use this skill whenever generating code for the PSMS project (frontend or backend). Follow these patterns exactly for consistency.

---

## BACKEND PATTERNS

### Project Structure

```
backend/src/
├── psms.Application/{Module}/{Entity}/
│   ├── Dto/
│   │   ├── {Entity}Dto.cs              (full detail DTO)
│   │   ├── {Entity}ListDto.cs          (lightweight list DTO)
│   │   ├── Create{Entity}Dto.cs        (create input)
│   │   ├── Update{Entity}Dto.cs        (update input)
│   │   └── Get{Entity}sInput.cs        (filter/paging input, optional)
│   ├── I{Entity}AppService.cs          (interface)
│   └── {Entity}AppService.cs           (implementation)
├── psms.Application/{Module}/Shared/
│   ├── {Module}Mapper.cs               (AutoMapper Profile)
│   └── {Module}ExceptionCodes.cs       (error codes)
├── psms.Core/Domain/{Module}/Entities/
│   └── {Entity}.cs                     (domain entity)
└── psms.Core/Authorization/
    └── PermissionNames.cs              (permission constants)
```

### Entity Pattern

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using psms.Domain.Shared.Enums;

namespace psms.Domain.{Module}.Entities;

[Table("{TableName}")]
public class {Entity} : FullAuditedEntity<Guid>, IMayHaveTenant, ISoftDelete
{
    public const int Max{Field}Length = 100;

    public int? TenantId { get; set; }

    [Required]
    [StringLength(Max{Field}Length)]
    public string {Field} { get; set; }

    public bool IsDeleted { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(RelatedId))]
    public virtual RelatedEntity Related { get; set; }
    public virtual ICollection<Child> Children { get; set; }

    // Protected constructor for EF Core
    protected {Entity}()
    {
        Children = new HashSet<Child>();
    }

    // Public constructor with required params
    public {Entity}(Guid id, int? tenantId, /* required params */) : this()
    {
        Id = id;
        TenantId = tenantId;
        IsDeleted = false;
    }

    // Domain methods for business logic (status transitions)
    public void StatusTransition()
    {
        if (Status == SomeStatus.Invalid)
            throw new InvalidOperationException("Cannot transition from this state.");
        Status = SomeStatus.Next;
    }
}
```

**Key rules:**
- Base class: `FullAuditedEntity<Guid>` (NOT `FullAuditedAggregateRoot`)
- Interfaces: `IMayHaveTenant` (`int? TenantId`), `ISoftDelete`
- Keys: always `Guid`
- TenantId: always `int?` (nullable)
- Junction tables (e.g., GradeSubject): use `Entity<Guid>`, NO `ISoftDelete`, NO `IMayHaveTenant`
- Collections initialized as `HashSet<T>()` in protected constructor
- Domain methods throw `InvalidOperationException` for invalid state transitions

### AppService Pattern

```csharp
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;

namespace psms.{Module}.{Entity}s;

[AbpAuthorize(PermissionNames.{Module}_{Entity}s)]
public class {Entity}AppService : ApplicationService, I{Entity}AppService
{
    private readonly IRepository<{Entity}, Guid> _{entity}Repository;

    public {Entity}AppService(IRepository<{Entity}, Guid> {entity}Repository)
    {
        _{entity}Repository = {entity}Repository;
    }

    [AbpAuthorize(PermissionNames.{Module}_{Entity}s_View)]
    public async Task<{Entity}Dto> GetAsync(Guid id)
    {
        var entity = await _{entity}Repository
            .GetAll()
            .Include(x => x.Related)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == AbpSession.TenantId);

        if (entity == null)
            throw new UserFriendlyException(ExceptionCodes.NotFound, "Not found.");

        return ObjectMapper.Map<{Entity}Dto>(entity);
    }

    [AbpAuthorize(PermissionNames.{Module}_{Entity}s_View)]
    public async Task<PagedResultDto<{Entity}ListDto>> GetAllAsync(PagedAndSortedResultRequestDto input)
    {
        var query = _{entity}Repository
            .GetAll()
            .Include(x => x.Related)
            .Where(x => x.TenantId == AbpSession.TenantId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<{Entity}ListDto>(
            totalCount,
            ObjectMapper.Map<List<{Entity}ListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.{Module}_{Entity}s_Manage)]
    public async Task<{Entity}Dto> CreateAsync(Create{Entity}Dto input)
    {
        // 1. Validate (duplicates, business rules, FK existence)
        var existing = await _{entity}Repository
            .FirstOrDefaultAsync(x => x.UniqueField == input.UniqueField
                && x.TenantId == AbpSession.TenantId);
        if (existing != null)
            throw new UserFriendlyException(ExceptionCodes.Duplicate, "Already exists.");

        // 2. Create entity via constructor
        var entity = new Entity(Guid.NewGuid(), AbpSession.TenantId, input.Field1)
        {
            OptionalField = input.OptionalField
        };

        // 3. Insert + save
        await _{entity}Repository.InsertAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        // 4. Return full DTO (reload with includes)
        return await GetAsync(entity.Id);
    }

    [AbpAuthorize(PermissionNames.{Module}_{Entity}s_Manage)]
    public async Task<{Entity}Dto> UpdateAsync(Guid id, Update{Entity}Dto input)
    {
        var entity = await _{entity}Repository.GetAsync(id);

        // Null-skip pattern for optional update fields
        if (input.Field != null) entity.Field = input.Field;
        if (input.NullableField.HasValue) entity.NullableField = input.NullableField.Value;

        await _{entity}Repository.UpdateAsync(entity);
        await CurrentUnitOfWork.SaveChangesAsync();

        return await GetAsync(id);
    }

    [AbpAuthorize(PermissionNames.{Module}_{Entity}s_Manage)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await _{entity}Repository
            .GetAll()
            .Include(x => x.Children)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == AbpSession.TenantId);

        if (entity == null)
            throw new UserFriendlyException(ExceptionCodes.NotFound, "Not found.");

        // Prevent deletion if active children exist
        if (entity.Children.Any(c => !c.IsDeleted))
            throw new UserFriendlyException(ExceptionCodes.HasChildren, "Cannot delete.");

        await _{entity}Repository.DeleteAsync(entity);
    }
}
```

**Key rules:**
- Base: `ApplicationService` + `IApplicationService` (NOT `AsyncCrudAppService`)
- Class-level `[AbpAuthorize]` + method-level for specific actions
- Constructor DI for repositories
- `ObjectMapper.Map<TDto>(entity)` for mapping
- `await CurrentUnitOfWork.SaveChangesAsync()` after every mutation
- `UserFriendlyException(ExceptionCode, "message")` for errors
- Tenant filter: `.Where(x => x.TenantId == AbpSession.TenantId)` on EVERY query
- `.WhereIf()` for conditional filters
- `.OrderBy(input.Sorting ?? "default").PageBy(input).ToListAsync()`
- Domain method errors: catch `InvalidOperationException`, rethrow as `UserFriendlyException`
- After create/update: return `await GetAsync(entity.Id)` to reload with includes

### Interface Pattern

```csharp
using Abp.Application.Services;
using Abp.Application.Services.Dto;

namespace psms.{Module}.{Entity}s;

public interface I{Entity}AppService : IApplicationService
{
    Task<{Entity}Dto> GetAsync(Guid id);
    Task<PagedResultDto<{Entity}ListDto>> GetAllAsync(PagedAndSortedResultRequestDto input);
    Task<{Entity}Dto> CreateAsync(Create{Entity}Dto input);
    Task<{Entity}Dto> UpdateAsync(Guid id, Update{Entity}Dto input);
    Task DeleteAsync(Guid id);
}
```

### DTO Patterns

**Create DTO** - validation attributes, required fields:
```csharp
namespace psms.{Module}.{Entity}s.Dto;

public class Create{Entity}Dto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }

    [Required]
    [Range(0.01, 9999999.99)]
    public decimal Amount { get; set; }

    [StringLength(500)]
    public string Description { get; set; }  // optional, no [Required]
}
```

**Update DTO** - all fields optional (null-skip):
```csharp
public class Update{Entity}Dto
{
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; }           // null = skip

    public bool? IsActive { get; set; }        // null = skip
}
```

**List DTO** - lightweight, inherits `EntityDto<Guid>`:
```csharp
public class {Entity}ListDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public int ChildCount { get; set; }        // computed in mapper
}
```

**Full DTO** - includes computed/flattened props, inherits `FullAuditedEntityDto<Guid>`:
```csharp
public class {Entity}Dto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; }
    // Flattened from navigation properties
    public string RelatedEntityName { get; set; }
    // Computed
    public int ChildCount { get; set; }
}
```

### Mapper Pattern

```csharp
using AutoMapper;

namespace psms.{Module}.Shared;

public class {Module}Mapper : Profile
{
    public {Module}Mapper()
    {
        Create{Entity}Mappings();
    }

    private void Create{Entity}Mappings()
    {
        // Entity -> Full DTO
        CreateMap<Entity, EntityDto>()
            .ForMember(d => d.ChildCount,
                o => o.MapFrom(s => s.Children != null ? s.Children.Count(c => !c.IsDeleted) : 0))
            .ForMember(d => d.RelatedName,
                o => o.MapFrom(s => s.Related != null ? s.Related.Name : null));

        // Entity -> List DTO
        CreateMap<Entity, EntityListDto>()
            .ForMember(d => d.ChildCount,
                o => o.MapFrom(s => s.Children != null ? s.Children.Count(c => !c.IsDeleted) : 0));

        // Update DTO -> Entity (null-skip)
        CreateMap<UpdateEntityDto, Entity>()
            .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));
    }
}
```

### Exception Codes Pattern

```csharp
namespace psms.{Module}.Shared;

public static class {Module}ExceptionCodes
{
    public const string {Entity}NotFound = "{MOD}_{ENTITY}_NOT_FOUND";
    public const string Duplicate{Entity} = "{MOD}_DUPLICATE_{ENTITY}";
    public const string CannotDelete{Entity}With{Children} = "{MOD}_CANNOT_DELETE_{ENTITY}_WITH_{CHILDREN}";
}
```

### Permission Names Pattern

```csharp
// Format: {Module}_{Entity}_{Action}
public const string {Module}_{Entity}s = "{Module}.{Entity}s";
public const string {Module}_{Entity}s_View = "{Module}.{Entity}s.View";
public const string {Module}_{Entity}s_Manage = "{Module}.{Entity}s.Manage";
```

---

## FRONTEND PATTERNS

### Project Structure

```
frontend/src/
├── app/                          (Next.js App Router pages)
│   ├── layout.tsx                (root layout)
│   ├── providers.tsx             (global providers wrapper)
│   ├── auth/login/page.tsx
│   ├── admin/
│   ├── principal/
│   ├── teacher/
│   └── {role}/{feature}/page.tsx
├── components/                   (reusable UI components)
│   ├── {feature}/
│   │   ├── {Feature}PageContent.tsx
│   │   └── {Feature}FormModal.tsx
│   └── shared/
│       └── EnterpriseTable/
├── providers/                    (state management by module)
│   ├── {module}/{entity}/
│   │   ├── context.tsx           (interfaces + initial state)
│   │   ├── actions.tsx           (redux-actions action creators)
│   │   ├── reducer.tsx           (handleActions reducer)
│   │   └── index.tsx             (Provider component + hooks)
│   └── {module}/shared/
│       └── interfaces.ts         (TypeScript interfaces)
└── utils/
    ├── axios-instance.ts         (axios config + interceptors)
    ├── theme-config.ts           (Ant Design theme)
    └── jwt-decoder.ts            (JWT utilities)
```

### Tech Stack

- **Framework:** Next.js 16 (App Router)
- **UI Library:** Ant Design v6
- **State:** React Context API + useReducer + redux-actions
- **HTTP:** Axios with interceptors
- **Validation:** Zod (MANDATORY for all forms)
- **Styling:** Ant Design theme + CSS Modules
- **Auth:** JWT in sessionStorage, `Abp-TenantId` header

### Provider Pattern (4 files per entity)

**context.tsx** - State and action interfaces:
```typescript
import { createContext } from "react";

export interface I{Entity}StateContext {
  isPending: boolean;
  isSuccess: boolean;
  isError: boolean;
  {entity}?: I{Entity};
  {entity}s?: I{Entity}List[];
  totalCount?: number;
}

export interface I{Entity}ActionContext {
  getAsync: (id: string) => void;
  getAllAsync: (input?: IPagedAndSortedResultRequest) => void;
  createAsync: (input: ICreate{Entity}) => void;
  updateAsync: (id: string, input: IUpdate{Entity}) => void;
  deleteAsync: (id: string) => void;
}

export const INITIAL_STATE: I{Entity}StateContext = {
  isPending: false,
  isSuccess: false,
  isError: false,
};

export const {Entity}StateContext = createContext<I{Entity}StateContext>(INITIAL_STATE);
export const {Entity}ActionContext = createContext<I{Entity}ActionContext | undefined>(undefined);
```

**actions.tsx** - Redux-actions action creators:
```typescript
import { createAction } from "redux-actions";
import { I{Entity}StateContext } from "./context";

export enum {Entity}ActionEnums {
  get{Entity}Pending = "GET_{ENTITY}_PENDING",
  get{Entity}Success = "GET_{ENTITY}_SUCCESS",
  get{Entity}Error = "GET_{ENTITY}_ERROR",
  getAll{Entity}sPending = "GET_ALL_{ENTITY}S_PENDING",
  getAll{Entity}sSuccess = "GET_ALL_{ENTITY}S_SUCCESS",
  getAll{Entity}sError = "GET_ALL_{ENTITY}S_ERROR",
  create{Entity}Pending = "CREATE_{ENTITY}_PENDING",
  create{Entity}Success = "CREATE_{ENTITY}_SUCCESS",
  create{Entity}Error = "CREATE_{ENTITY}_ERROR",
  update{Entity}Pending = "UPDATE_{ENTITY}_PENDING",
  update{Entity}Success = "UPDATE_{ENTITY}_SUCCESS",
  update{Entity}Error = "UPDATE_{ENTITY}_ERROR",
  delete{Entity}Pending = "DELETE_{ENTITY}_PENDING",
  delete{Entity}Success = "DELETE_{ENTITY}_SUCCESS",
  delete{Entity}Error = "DELETE_{ENTITY}_ERROR",
}

export const get{Entity}Pending = createAction<I{Entity}StateContext>(
  {Entity}ActionEnums.get{Entity}Pending,
  () => ({ isPending: true, isSuccess: false, isError: false })
);

export const get{Entity}Success = createAction<I{Entity}StateContext, I{Entity}>(
  {Entity}ActionEnums.get{Entity}Success,
  ({entity}) => ({ isPending: false, isSuccess: true, isError: false, {entity} })
);

export const get{Entity}Error = createAction<I{Entity}StateContext>(
  {Entity}ActionEnums.get{Entity}Error,
  () => ({ isPending: false, isSuccess: false, isError: true })
);

// ... repeat for getAll, create, update, delete
```

**reducer.tsx** - handleActions reducer:
```typescript
import { handleActions } from "redux-actions";
import { I{Entity}StateContext, INITIAL_STATE } from "./context";
import { {Entity}ActionEnums } from "./actions";

export const {Entity}Reducer = handleActions<I{Entity}StateContext, I{Entity}StateContext>(
  {
    [{Entity}ActionEnums.get{Entity}Pending]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [{Entity}ActionEnums.get{Entity}Success]: (state, action) => ({
      ...state, ...action.payload,
    }),
    [{Entity}ActionEnums.get{Entity}Error]: (state, action) => ({
      ...state, ...action.payload,
    }),
    // ... all action handlers follow same spread pattern
  },
  INITIAL_STATE
);
```

**index.tsx** - Provider component + custom hooks:
```typescript
"use client";

import React, { useReducer, useContext } from "react";
import { getAxiosInstance } from "@/utils/axios-instance";
import {
  {Entity}StateContext,
  {Entity}ActionContext,
  INITIAL_STATE,
} from "./context";
import { {Entity}Reducer } from "./reducer";
import {
  get{Entity}Pending, get{Entity}Success, get{Entity}Error,
  getAll{Entity}sPending, getAll{Entity}sSuccess, getAll{Entity}sError,
  create{Entity}Pending, create{Entity}Success, create{Entity}Error,
  update{Entity}Pending, update{Entity}Success, update{Entity}Error,
  delete{Entity}Pending, delete{Entity}Success, delete{Entity}Error,
} from "./actions";

export const {Entity}Provider = ({ children }: { children: React.ReactNode }) => {
  const [state, dispatch] = useReducer({Entity}Reducer, INITIAL_STATE);
  const instance = getAxiosInstance();

  const getAsync = async (id: string) => {
    dispatch(get{Entity}Pending());
    const endpoint = `/api/services/app/{Entity}/Get?id=${id}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(get{Entity}Success(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(get{Entity}Error());
      });
  };

  const getAllAsync = async (input?: IPagedAndSortedResultRequest) => {
    dispatch(getAll{Entity}sPending());
    const params = new URLSearchParams();
    if (input?.maxResultCount) params.append("MaxResultCount", input.maxResultCount.toString());
    if (input?.skipCount) params.append("SkipCount", input.skipCount.toString());
    if (input?.sorting) params.append("Sorting", input.sorting);

    const endpoint = `/api/services/app/{Entity}/GetAll?${params.toString()}`;
    await instance
      .get(endpoint)
      .then((response) => {
        dispatch(getAll{Entity}sSuccess({
          items: response.data.result.items,
          totalCount: response.data.result.totalCount,
        }));
      })
      .catch((error) => {
        console.error(error);
        dispatch(getAll{Entity}sError());
      });
  };

  const createAsync = async (input: ICreate{Entity}) => {
    dispatch(create{Entity}Pending());
    const endpoint = `/api/services/app/{Entity}/Create`;
    await instance
      .post(endpoint, input)
      .then((response) => {
        dispatch(create{Entity}Success(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(create{Entity}Error());
      });
  };

  const updateAsync = async (id: string, input: IUpdate{Entity}) => {
    dispatch(update{Entity}Pending());
    const endpoint = `/api/services/app/{Entity}/Update`;
    await instance
      .put(endpoint, { id, ...input })
      .then((response) => {
        dispatch(update{Entity}Success(response.data.result));
      })
      .catch((error) => {
        console.error(error);
        dispatch(update{Entity}Error());
      });
  };

  const deleteAsync = async (id: string) => {
    dispatch(delete{Entity}Pending());
    const endpoint = `/api/services/app/{Entity}/Delete?id=${id}`;
    await instance
      .delete(endpoint)
      .then(() => {
        dispatch(delete{Entity}Success());
      })
      .catch((error) => {
        console.error(error);
        dispatch(delete{Entity}Error());
      });
  };

  return (
    <{Entity}StateContext.Provider value={state}>
      <{Entity}ActionContext.Provider value={{ getAsync, getAllAsync, createAsync, updateAsync, deleteAsync }}>
        {children}
      </{Entity}ActionContext.Provider>
    </{Entity}StateContext.Provider>
  );
};

export const use{Entity}State = () => {
  const context = useContext({Entity}StateContext);
  if (!context) throw new Error("use{Entity}State must be used within a {Entity}Provider");
  return context;
};

export const use{Entity}Actions = () => {
  const context = useContext({Entity}ActionContext);
  if (!context) throw new Error("use{Entity}Actions must be used within a {Entity}Provider");
  return context;
};
```

### TypeScript Interface Pattern

```typescript
// providers/{module}/shared/interfaces.ts

export interface IPagedAndSortedResultRequest {
  maxResultCount?: number;
  skipCount?: number;
  sorting?: string;
}

export interface I{Entity} {
  id: string;
  name: string;
  // ... all fields from full DTO
}

export interface I{Entity}List {
  id: string;
  name: string;
  // ... lightweight fields from list DTO
}

export interface ICreate{Entity} {
  name: string;
  // ... required fields only
}

export interface IUpdate{Entity} {
  name?: string;
  // ... all optional fields
}
```

### API Endpoint Convention

```
GET    /api/services/app/{Entity}/Get?id={id}
GET    /api/services/app/{Entity}/GetAll?MaxResultCount=10&SkipCount=0&Sorting=Name
POST   /api/services/app/{Entity}/Create          (body: CreateDto)
PUT    /api/services/app/{Entity}/Update           (body: { id, ...UpdateDto })
DELETE /api/services/app/{Entity}/Delete?id={id}
POST   /api/services/app/{Entity}/{CustomAction}   (body: input)
```

### Axios Instance

```typescript
import axios from "axios";

export const getAxiosInstance = () => {
  const instance = axios.create({
    baseURL: process.env.NEXT_PUBLIC_BASE_URL,
    headers: { "Content-Type": "application/json;charset=utf-8" },
  });

  instance.interceptors.request.use((config) => {
    config.headers = config.headers ?? {};
    const tenantId = sessionStorage.getItem("tenantId");
    if (tenantId) config.headers["Abp-TenantId"] = tenantId;
    const token = sessionStorage.getItem("accessToken");
    if (token) config.headers["Authorization"] = `Bearer ${token}`;
    return config;
  });

  return instance;
};
```

### Form Modal Pattern (with Zod validation - MANDATORY)

```typescript
"use client";

import React, { useEffect, useState } from "react";
import { Modal, Form, Input, InputNumber, Select, message } from "antd";
import { z } from "zod";
import { use{Entity}Actions } from "@/providers/{module}/{entity}";

// Zod schema (REQUIRED for all forms)
const {entity}Schema = z.object({
  name: z.string().min(1, "Name is required").max(100),
  amount: z.number().min(0.01, "Must be positive"),
  description: z.string().max(500).optional(),
});

interface {Entity}FormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: I{Entity}List | null;
}

export const {Entity}FormModal: React.FC<{Entity}FormModalProps> = ({
  open, onClose, editRecord,
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = use{Entity}Actions();
  const [loading, setLoading] = useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      if (editRecord) {
        form.setFieldsValue({ name: editRecord.name });
      } else {
        form.resetFields();
      }
    }
  }, [open, editRecord, form]);

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      const result = {entity}Schema.safeParse(values);

      if (!result.success) {
        const fieldErrors = result.error.issues.map((err) => ({
          name: err.path as string[],
          errors: [err.message],
        }));
        form.setFields(fieldErrors);
        return;
      }

      setLoading(true);
      if (isEdit) {
        await updateAsync(editRecord!.id, result.data);
      } else {
        await createAsync(result.data);
      }
      message.success(`${isEdit ? "Updated" : "Created"} successfully`);
      onClose(true);
    } catch {
      message.error("An error occurred");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? "Edit {Entity}" : "New {Entity}"}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Name" name="name" rules={[{ required: true }]}>
          <Input placeholder="Enter name" maxLength={100} />
        </Form.Item>
      </Form>
    </Modal>
  );
};
```

### Page Content Pattern (with EnterpriseTable)

```typescript
"use client";

import React, { useState, useCallback } from "react";
import { message } from "antd";
import { PlusOutlined, EditOutlined, DeleteOutlined } from "@ant-design/icons";
import { use{Entity}State, use{Entity}Actions, {Entity}Provider } from "@/providers/{module}/{entity}";
import { useAuthState } from "@/providers/auth";
import EnterpriseTable from "@/components/shared/EnterpriseTable";
import { {Entity}FormModal } from "./{Entity}FormModal";

function {Entity}Content() {
  const { {entity}s, totalCount, isPending, isError } = use{Entity}State();
  const { getAllAsync, deleteAsync } = use{Entity}Actions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<I{Entity}List | null>(null);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

  const handleQueryChange = useCallback((query: TableQuery) => {
    setLastQuery(query);
    getAllAsync({
      maxResultCount: query.maxResultCount,
      skipCount: query.skipCount,
      sorting: query.sorting,
    });
  }, [getAllAsync]);

  const refreshData = () => {
    if (lastQuery) handleQueryChange(lastQuery);
  };

  const handleModalClose = (refresh?: boolean) => {
    setModalOpen(false);
    setEditRecord(null);
    if (refresh) refreshData();
  };

  const columns = [
    { key: "name", title: "Name", dataIndex: "name", sortable: true, filterable: true },
    { key: "isActive", title: "Status", dataIndex: "isActive", renderType: "status" },
  ];

  const toolbarActions = [
    {
      key: "new", label: "New", icon: <PlusOutlined />, type: "primary",
      onClick: () => { setEditRecord(null); setModalOpen(true); },
    },
  ];

  const rowActions = [
    {
      key: "edit", label: "Edit", icon: <EditOutlined />,
      onClick: (record) => { setEditRecord(record); setModalOpen(true); },
    },
    {
      key: "delete", label: "Delete", icon: <DeleteOutlined />, danger: true,
      confirm: { title: "Delete?", description: "This cannot be undone." },
      onClick: async (record) => {
        await deleteAsync(record.id);
        message.success("Deleted");
        refreshData();
      },
    },
  ];

  return (
    <>
      <EnterpriseTable
        title="{Entity}s"
        columns={columns}
        data={{entity}s ?? []}
        totalCount={totalCount}
        loading={isPending}
        error={isError}
        onQueryChange={handleQueryChange}
        rowKey="id"
        toolbarActions={toolbarActions}
        rowActions={rowActions}
        currentUserRole={currentRole}
      />
      <{Entity}FormModal open={modalOpen} onClose={handleModalClose} editRecord={editRecord} />
    </>
  );
}

// Page export wraps content in provider
export default function {Entity}PageContent() {
  return (
    <{Entity}Provider>
      <{Entity}Content />
    </{Entity}Provider>
  );
}
```

### Page File Pattern (simple delegation)

```typescript
// src/app/{role}/{feature}/page.tsx
"use client";

export { default } from "@/components/{feature}/{Feature}PageContent";
```

### Auth Pattern

- Token stored in `sessionStorage` (not localStorage)
- Tenant resolved via `/api/services/app/Account/IsTenantAvailable`
- Auth via `/api/TokenAuth/Authenticate`
- JWT decoded for role with `jwt-decode`
- Role-based routing after login (admin, principal, teacher, student, parent)
- `Abp-TenantId` header set on every request via axios interceptor
- Protected routes check `jwtToken` and `currentRole`

### Styling

- Ant Design v6 with ConfigProvider theme
- Enterprise/desktop aesthetic (Oracle/SAP inspired)
- CSS Modules for component-specific styles
- No CSS-in-JS libraries
- Minimal border radius, tight spacing
- Theme colors: primary `#0066CC`, header `#003D73`
