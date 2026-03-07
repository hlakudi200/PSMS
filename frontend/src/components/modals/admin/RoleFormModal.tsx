'use client';

import React, { useEffect, useMemo, useState } from 'react';
import { Modal, Form, Input, Tree, message, Spin } from 'antd';
import { z } from 'zod';
import { useRoleActions, useRoleState } from '@/providers/admin/roles';
import type { IAdminRole, IPermissionDto } from '@/providers/admin/shared/interfaces';
import type { DataNode } from 'antd/es/tree';

const roleSchema = z.object({
  name: z.string().min(1, 'Name is required').max(32),
  displayName: z.string().min(1, 'Display name is required').max(64),
  description: z.string().max(500).optional(),
  grantedPermissions: z.array(z.string()),
});

interface RoleFormModalProps {
  open: boolean;
  onClose: (refreshData?: boolean) => void;
  editRecord?: IAdminRole | null;
}

function buildPermissionTree(permissions: IPermissionDto[]): DataNode[] {
  const map = new Map<string, DataNode & { children: DataNode[] }>();
  const roots: DataNode[] = [];

  for (const p of permissions) {
    map.set(p.name, { key: p.name, title: p.displayName, children: [] });
  }

  for (const p of permissions) {
    const node = map.get(p.name)!;
    if (p.parentName && map.has(p.parentName)) {
      (map.get(p.parentName)!.children as DataNode[]).push(node);
    } else {
      roots.push(node);
    }
  }

  return roots;
}

export const RoleFormModal: React.FC<RoleFormModalProps> = ({
  open,
  onClose,
  editRecord,
}) => {
  const [form] = Form.useForm();
  const { createAsync, updateAsync, getAllPermissionsAsync, getRoleForEditAsync } = useRoleActions();
  const { allPermissions, roleForEdit, isPending } = useRoleState();
  const [loading, setLoading] = useState(false);
  const [checkedKeys, setCheckedKeys] = useState<string[]>([]);
  const isEdit = !!editRecord;

  useEffect(() => {
    if (open) {
      getAllPermissionsAsync();
      if (editRecord) {
        getRoleForEditAsync(editRecord.id);
      } else {
        form.resetFields();
        setCheckedKeys([]);
      }
    }
  }, [open, editRecord, form, getAllPermissionsAsync, getRoleForEditAsync]);

  useEffect(() => {
    if (open && isEdit && roleForEdit) {
      form.setFieldsValue({
        name: roleForEdit.role.name,
        displayName: roleForEdit.role.displayName,
        description: roleForEdit.role.description,
      });
      setCheckedKeys(roleForEdit.grantedPermissionNames);
    }
  }, [open, isEdit, roleForEdit, form]);

  const treeData = useMemo(
    () => buildPermissionTree(allPermissions ?? []),
    [allPermissions]
  );

  const handleSubmit = async () => {
    try {
      const values = form.getFieldsValue();
      const data = { ...values, grantedPermissions: checkedKeys };
      const result = roleSchema.safeParse(data);

      if (!result.success) {
        const fieldErrors = result.error.issues.map(err => ({
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
      message.success(`Role ${isEdit ? 'updated' : 'created'} successfully`);
      onClose(true);
    } catch {
      message.error('An error occurred');
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal
      title={isEdit ? 'Edit Role' : 'New Role'}
      open={open}
      onCancel={() => onClose()}
      onOk={handleSubmit}
      confirmLoading={loading}
      destroyOnClose
      width={720}
    >
      <Form form={form} layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Name" name="name" rules={[{ required: true }]}>
          <Input
            placeholder="Role name"
            maxLength={32}
            disabled={isEdit && editRecord?.isStatic}
          />
        </Form.Item>
        <Form.Item label="Display Name" name="displayName" rules={[{ required: true }]}>
          <Input placeholder="Display name" maxLength={64} />
        </Form.Item>
        <Form.Item label="Description" name="description">
          <Input.TextArea rows={3} maxLength={500} placeholder="Optional description" />
        </Form.Item>
        <Form.Item label="Permissions">
          {isPending ? (
            <Spin />
          ) : (
            <div style={{ maxHeight: 400, overflow: 'auto', border: '1px solid #d9d9d9', borderRadius: 6, padding: 8 }}>
              <Tree
                checkable
                treeData={treeData}
                checkedKeys={checkedKeys}
                onCheck={(checked) => {
                  setCheckedKeys(
                    Array.isArray(checked) ? checked as string[] : checked.checked as string[]
                  );
                }}
                defaultExpandAll
              />
            </div>
          )}
        </Form.Item>
      </Form>
    </Modal>
  );
};
