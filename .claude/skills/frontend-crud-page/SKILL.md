---
name: frontend-crud-page
description: The full-stack frontend CRUD page pattern — a feature provider (Context+useReducer) + the shared EnterpriseTable (columns, paging, search, row/bulk actions, export) + a Zod-validated Ant Design form modal, tied together by a PageContent component that loads on query change and refreshes after mutations. Use when building a list/detail/CRUD screen in the Next.js + Ant Design app.
---

# Frontend CRUD Page

The standard recipe for a list-and-manage screen: **Provider → EnterpriseTable → Zod form modal**, assembled by a `PageContent` component. Builds on the **react-state-management** skill (the provider) and pairs with the **abp-backend-crud** endpoints. All forms validate with **Zod** (project rule).

Substitute `Foo` for your entity. Canonical references in this repo: `RolesPageContent` (simple) and `ClassesPageContent` (bulk actions + nested providers + detail nav).

## Folder layout

```
src/app/<role>/foos/page.tsx                         # thin re-export of the content component
src/components/modules/<module>/FoosPageContent.tsx  # ties provider + table + modal
src/components/modals/<module>/FooFormModal.tsx       # Zod + antd Form create/edit
src/providers/<module>/foos/{index,context,actions,reducer}.tsx   # the data provider (see react-state-management)
src/providers/<module>/shared/interfaces.ts           # IFoo, ICreateFoo, IUpdateFoo, IGetFoosInput
src/components/shared/enterprise-table/               # shared, already exists — just consume it
```

`page.tsx` is a one-liner so the route file stays trivial:

```tsx
import FoosPageContent from '@/components/modules/<module>/FoosPageContent';
export default FoosPageContent;
```

## 1. EnterpriseTable — the shared list component

Don't rebuild tables. `EnterpriseTable<T>` handles paging, sorting, search, row actions, bulk actions, export, and permission-gated columns. Key props:

```ts
interface EnterpriseTableProps<T> {
  columns: ColumnConfig<T>[];
  data: T[];
  totalCount?: number;
  loading?: boolean;
  error?: boolean;
  onQueryChange: (q: TableQuery) => void;   // fires on page/sort/filter — you call getAllAsync here
  rowKey: string;
  title?: string;
  toolbarActions?: ToolbarAction[];          // e.g. the "New" button
  rowActions?: RowAction<T>[];               // edit / delete (with confirm) / view
  bulkActions?: BulkAction<T>[];             // multi-select operations
  selectionMode?: 'none' | 'single' | 'multi';
  searchable?: boolean; searchPlaceholder?: string; searchFilterKey?: string;
  exportConfig?: { enabled: boolean; formats: ('csv'|'xlsx')[] };
  currentUserRole?: string;                  // drives permission-gated columns/actions
}
```

`ColumnConfig` supports `renderType: 'status'|'date'|'datetime'|'money'|'boolean'|'masked'`, `sortable`, `filterable`, custom `render`, and `requiredPermissions`. `RowAction` supports `confirm: { title, description }`, `danger`, and `disabled: (record) => boolean`.

## 2. Zod form modal — `FooFormModal.tsx`

The house pattern: **define the Zod schema(s) outside the component**, validate with `safeParse` on submit, and map Zod issues onto Ant Design fields with `form.setFields`. Use **separate schemas for create vs edit** when the editable fields differ.

```tsx
const fooSchema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
  code: z.string().min(1, 'Code is required').max(20),
  isCore: z.boolean(),
});
const editSchema = fooSchema.pick({ name: true });   // narrower on edit, if needed

interface FooFormModalProps { open: boolean; onClose: (refresh?: boolean) => void; editRecord?: IFoo | null; }

export const FooFormModal: React.FC<FooFormModalProps> = ({ open, onClose, editRecord }) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync } = useFooActions();
  const [loading, setLoading] = useState(false);
  const isEdit = !!editRecord;

  useEffect(() => {                                   // populate / reset on open
    if (!open) return;
    if (editRecord) form.setFieldsValue(editRecord);
    else form.resetFields();
  }, [open, editRecord, form]);

  const handleSubmit = async () => {
    const values = form.getFieldsValue();
    const schema = isEdit ? editSchema : fooSchema;
    const result = schema.safeParse(values);
    if (!result.success) {                            // Zod → antd field errors
      form.setFields(result.error.issues.map(i => ({ name: i.path as string[], errors: [i.message] })));
      return;
    }
    setLoading(true);
    try {
      if (isEdit) await updateAsync(editRecord!.id, result.data);
      else        await createAsync(result.data);
      message.success(`Foo ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);                                  // true → parent refreshes the list
    } catch {
      /* axios interceptor already showed the error modal */
    } finally { setLoading(false); }
  };

  return (
    <Modal title={isEdit ? 'Edit Foo' : 'New Foo'} open={open} onCancel={() => onClose()}
           onOk={handleSubmit} confirmLoading={loading} destroyOnClose>
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Name" name="name" rules={[{ required: true }]}><Input maxLength={100} /></Form.Item>
        <Form.Item label="Code" name="code" rules={[{ required: true }]}><Input maxLength={20} /></Form.Item>
        <Form.Item label="Core" name="isCore" valuePropName="checked"><Switch /></Form.Item>
      </Form>
    </Modal>
  );
};
```

> The `rules` on `Form.Item` give inline UX hints; **Zod `safeParse` is the source of truth** before submit (CLAUDE.md mandates Zod). Don't rely on antd rules alone.

## 3. PageContent — ties it together

Two components in one file: an inner `FoosContent` that uses the provider hooks, wrapped by an exported `FoosPageContent` that mounts the provider(s).

```tsx
'use client';
function FoosContent() {
  const { foos, totalCount, isPending, isError } = useFooState();
  const { getAllAsync, deleteAsync } = useFooActions();
  const { currentRole } = useAuthState();
  const [modalOpen, setModalOpen] = useState(false);
  const [editRecord, setEditRecord] = useState<IFoo | null>(null);
  const [lastQuery, setLastQuery] = useState<TableQuery | null>(null);

  const handleQueryChange = useCallback((q: TableQuery) => {
    setLastQuery(q);
    getAllAsync({ maxResultCount: q.maxResultCount, skipCount: q.skipCount,
                  sorting: q.sorting, keyword: q.filters?.keyword as string | undefined });
  }, [getAllAsync]);

  const refreshData = useCallback(() => { if (lastQuery) handleQueryChange(lastQuery); }, [lastQuery, handleQueryChange]);
  const handleModalClose = (refresh?: boolean) => { setModalOpen(false); setEditRecord(null); if (refresh) refreshData(); };

  const columns: ColumnConfig<IFoo>[] = [
    { key: 'name', title: 'Name', dataIndex: 'name', sortable: true },
    { key: 'code', title: 'Code', dataIndex: 'code', sortable: true },
    { key: 'isActive', title: 'Active', dataIndex: 'isActive', renderType: 'boolean' },
  ];
  const toolbarActions: ToolbarAction[] = [
    { key: 'new', label: 'New Foo', icon: <PlusOutlined />, type: 'primary',
      onClick: () => { setEditRecord(null); setModalOpen(true); } },
  ];
  const rowActions: RowAction<IFoo>[] = [
    { key: 'edit', label: 'Edit', icon: <EditOutlined />,
      onClick: (r) => { setEditRecord(r); setModalOpen(true); } },
    { key: 'delete', label: 'Delete', icon: <DeleteOutlined />, danger: true,
      confirm: { title: 'Delete this foo?', description: 'This cannot be undone.' },
      onClick: async (r) => { await deleteAsync(r.id); message.success('Foo deleted'); refreshData(); } },
  ];

  return (
    <>
      <EnterpriseTable<IFoo>
        title="Foos" columns={columns} data={foos ?? []} totalCount={totalCount}
        loading={isPending} error={isError} onQueryChange={handleQueryChange} rowKey="id"
        toolbarActions={toolbarActions} rowActions={rowActions} selectionMode="none"
        currentUserRole={currentRole} searchable searchPlaceholder="Search foos..." searchFilterKey="keyword"
        exportConfig={{ enabled: true, formats: ['csv', 'xlsx'] }}
      />
      <FooFormModal open={modalOpen} onClose={handleModalClose} editRecord={editRecord} />
    </>
  );
}

export default function FoosPageContent() {
  return (<FooProvider><FoosContent /></FooProvider>);
}
```

## Conventions & gotchas

1. **Two-component split** — inner content uses provider hooks; the exported wrapper mounts the provider. The wrapper exists so hooks always run *inside* the provider.
2. **`onQueryChange` is the single load path.** Store the last query so `refreshData()` can replay it after create/update/delete. The table fires it on mount and on every page/sort/filter change.
3. **Modal owns nothing about the list.** It just calls `create/updateAsync` and signals `onClose(true)`; the parent re-fetches. State doesn't auto-refresh (see react-state-management rule 3).
4. **`editRecord` is the create/edit switch** — `null` ⇒ create, a record ⇒ edit.
5. **Errors are centralized** in the axios interceptor — the modal's `catch` is empty on purpose; don't double-toast.
6. **Nest providers** when the form needs lookups from other domains: `<FooProvider><GradeProvider><AcademicYearProvider><FoosContent/>…`. Each loads its own list in an effect.
7. **`currentUserRole`** from `useAuthState()` drives permission-gated columns/actions — pass it through.
8. **Bulk + multi-select**: set `selectionMode="multi"` and pass `bulkActions` (each with optional `confirm`); run them over the selected rows and `refreshData()` after.
9. **Detail navigation**: add a `view` row action that does `router.push('/<role>/foos/' + record.id)` instead of opening the modal.

## Dependencies
`antd`, `zod`, `@ant-design/icons`, the shared `EnterpriseTable`, the feature provider + `useAuthState`. See **react-state-management** for the provider files and **abp-react-auth** for the axios instance.
