import { useState } from 'react';
import { Card, CardContent } from '../components/ui';

const ROLES = [
  { id: 'admin', name: 'Admin', description: 'Full access to all features' },
  { id: 'accountant', name: 'Accountant', description: 'Access to invoices, reports, and customers' },
  { id: 'sales', name: 'Sales', description: 'Access to invoices and customers' },
  { id: 'manager', name: 'Manager', description: 'Access to reports and dashboards' },
  { id: 'viewer', name: 'Viewer', description: 'Read-only access' },
];

const PERMISSIONS = [
  'Dashboard.View',
  'Customer.Create',
  'Customer.Edit',
  'Customer.Delete',
  'Product.Create',
  'Product.Edit',
  'Product.Delete',
  'Invoice.Create',
  'Invoice.Edit',
  'Invoice.Delete',
  'Invoice.View',
  'Payment.Create',
  'Payment.View',
  'Report.View',
  'Vendor.Create',
  'Vendor.Edit',
  'Vendor.Delete',
  'Expense.Create',
  'Expense.View',
  'Inventory.Manage',
  'Data.Import',
  'Data.Export',
  'Permission.Manage',
  'User.Manage',
];

const ROLE_PERMISSIONS: Record<string, string[]> = {
  admin: PERMISSIONS,
  accountant: [
    'Dashboard.View',
    'Customer.Create',
    'Customer.Edit',
    'Customer.Delete',
    'Product.Create',
    'Product.Edit',
    'Invoice.Create',
    'Invoice.Edit',
    'Invoice.Delete',
    'Invoice.View',
    'Payment.Create',
    'Payment.View',
    'Report.View',
    'Expense.Create',
    'Expense.View',
    'Data.Export',
  ],
  sales: [
    'Dashboard.View',
    'Customer.Create',
    'Customer.Edit',
    'Product.Create',
    'Invoice.Create',
    'Invoice.Edit',
    'Invoice.View',
    'Payment.Create',
    'Payment.View',
    'Report.View',
  ],
  manager: ['Dashboard.View', 'Report.View', 'Expense.View', 'Invoice.View', 'Payment.View'],
  viewer: ['Dashboard.View', 'Report.View', 'Invoice.View', 'Payment.View', 'Customer.Edit'],
};

export default function PermissionsPage() {
  const [selectedRole, setSelectedRole] = useState('admin');

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">Permissions & Roles</h1>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        {/* Roles List */}
        <div>
          <h2 className="text-sm font-semibold text-gray-900 dark:text-white mb-3">Roles</h2>
          <div className="space-y-2">
            {ROLES.map((role) => (
              <button
                key={role.id}
                onClick={() => setSelectedRole(role.id)}
                className={`w-full text-left px-4 py-3 rounded-lg transition ${
                  selectedRole === role.id
                    ? 'bg-blue-600 text-white'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
                }`}
              >
                <p className="font-medium">{role.name}</p>
                <p className="text-xs opacity-75">{role.description}</p>
              </button>
            ))}
          </div>
        </div>

        {/* Permissions Matrix */}
        <div className="lg:col-span-3">
          <Card>
            <CardContent className="p-6">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                {ROLES.find((r) => r.id === selectedRole)?.name} Permissions
              </h2>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                {PERMISSIONS.map((perm) => {
                  const hasPermission = ROLE_PERMISSIONS[selectedRole]?.includes(perm);
                  return (
                    <label key={perm} className="flex items-center gap-3 p-3 rounded-lg bg-gray-50 dark:bg-slate-800/30 hover:bg-gray-100 dark:hover:bg-slate-800/50 transition cursor-pointer">
                      <input
                        type="checkbox"
                        checked={hasPermission}
                        readOnly
                        className="rounded w-4 h-4 text-blue-600"
                      />
                      <span className="text-sm text-gray-700 dark:text-slate-300">{perm}</span>
                    </label>
                  );
                })}
              </div>

              <div className="mt-6 p-4 bg-blue-50 dark:bg-blue-600/10 rounded-lg">
                <p className="text-sm text-gray-700 dark:text-slate-300">
                  <strong>{selectedRole.charAt(0).toUpperCase() + selectedRole.slice(1)}</strong> has{' '}
                  <strong>{ROLE_PERMISSIONS[selectedRole]?.length ?? 0}</strong> out of{' '}
                  <strong>{PERMISSIONS.length}</strong> permissions
                </p>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
