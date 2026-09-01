import { useState } from 'react';
import toast from 'react-hot-toast';
import { Download, Upload } from 'lucide-react';
import { Button, Card, CardContent } from '../components/ui';

const EXPORT_OPTIONS = [
  { label: 'Export Products (Excel)', endpoint: '/api/export/products?format=excel' },
  { label: 'Export Products (CSV)', endpoint: '/api/export/products?format=csv' },
  { label: 'Export Customers (Excel)', endpoint: '/api/export/customers?format=excel' },
  { label: 'Export Customers (CSV)', endpoint: '/api/export/customers?format=csv' },
  { label: 'Export Vendors (Excel)', endpoint: '/api/export/vendors?format=excel' },
  { label: 'Export Vendors (CSV)', endpoint: '/api/export/vendors?format=csv' },
  { label: 'Export Invoices (Excel)', endpoint: '/api/export/invoices?format=excel' },
];

const TEMPLATE_OPTIONS = [
  { label: 'Products Template', endpoint: '/api/import/templates/products' },
  { label: 'Customers Template', endpoint: '/api/import/templates/customers' },
  { label: 'Vendors Template', endpoint: '/api/import/templates/vendors' },
  { label: 'Opening Stock Template', endpoint: '/api/import/templates/stock' },
];

export default function ImportExportPage() {
  const [uploading, setUploading] = useState(false);
  const [importType, setImportType] = useState<'products' | 'customers' | 'vendors' | 'stock'>('products');

  const handleExport = async (endpoint: string) => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(endpoint, {
        headers: { 'Authorization': `Bearer ${token}` },
      });

      if (!response.ok) throw new Error('Export failed');

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `export-${Date.now()}${endpoint.includes('.csv') ? '.csv' : '.xlsx'}`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
      toast.success('Export downloaded');
    } catch (error) {
      toast.error('Export failed');
    }
  };

  const handleDownloadTemplate = async (endpoint: string) => {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(endpoint, {
        headers: { 'Authorization': `Bearer ${token}` },
      });

      if (!response.ok) throw new Error('Template download failed');

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `template-${Date.now()}.xlsx`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
      toast.success('Template downloaded');
    } catch (error) {
      toast.error('Template download failed');
    }
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>, type: string) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setUploading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);

      const token = localStorage.getItem('token');
      const response = await fetch(`/api/import/${type}`, {
        method: 'POST',
        headers: { 'Authorization': `Bearer ${token}` },
        body: formData,
      });

      const result = await response.json();

      if (response.ok) {
        toast.success(`Imported: ${result.successRows} success, ${result.errorRows} errors`);
      } else {
        toast.error(`Import failed: ${result.error}`);
      }
    } catch (error) {
      toast.error('Upload failed');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">Import / Export</h1>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Import Section */}
        <div>
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Import Data</h2>

          <Card className="mb-6">
            <CardContent className="p-6">
              <p className="text-sm text-gray-600 dark:text-slate-400 mb-4">Choose what to import:</p>
              <div className="space-y-2 mb-6">
                {['products', 'customers', 'vendors', 'stock'].map((type) => (
                  <label key={type} className="flex items-center gap-3">
                    <input
                      type="radio"
                      name="importType"
                      value={type}
                      checked={importType === type}
                      onChange={(e) => setImportType(e.target.value as any)}
                      className="rounded"
                    />
                    <span className="text-gray-700 dark:text-slate-300 capitalize">{type.replace('stock', 'Opening Stock')}</span>
                  </label>
                ))}
              </div>

              <div className="border-2 border-dashed border-gray-300 dark:border-slate-600 rounded-lg p-6 text-center">
                <Upload className="h-8 w-8 text-gray-400 mx-auto mb-2" />
                <p className="text-sm text-gray-600 dark:text-slate-400 mb-4">
                  Drop file here or click to upload
                </p>
                <input
                  type="file"
                  id="file-upload"
                  accept=".xlsx,.csv"
                  onChange={(e) => handleFileUpload(e, importType)}
                  className="hidden"
                  disabled={uploading}
                />
                <label htmlFor="file-upload">
                  <Button as="span" disabled={uploading} loading={uploading}>
                    Choose File
                  </Button>
                </label>
              </div>

              <p className="text-xs text-gray-500 dark:text-slate-500 mt-4 text-center">
                Supported formats: .xlsx, .csv
              </p>
            </CardContent>
          </Card>

          <div>
            <h3 className="text-sm font-semibold text-gray-900 dark:text-white mb-3">Download Templates</h3>
            <div className="grid grid-cols-1 gap-2">
              {TEMPLATE_OPTIONS.map((opt) => (
                <button
                  key={opt.endpoint}
                  onClick={() => handleDownloadTemplate(opt.endpoint)}
                  className="flex items-center gap-2 px-4 py-2 text-sm text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-600/20 rounded-lg transition"
                >
                  <Download className="h-4 w-4" />
                  {opt.label}
                </button>
              ))}
            </div>
          </div>
        </div>

        {/* Export Section */}
        <div>
          <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Export Data</h2>

          <Card>
            <CardContent className="p-6">
              <p className="text-sm text-gray-600 dark:text-slate-400 mb-4">Download data in multiple formats:</p>
              <div className="grid grid-cols-1 gap-2">
                {EXPORT_OPTIONS.map((opt) => (
                  <button
                    key={opt.endpoint}
                    onClick={() => handleExport(opt.endpoint)}
                    className="flex items-center gap-2 px-4 py-2 text-sm text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-600/20 rounded-lg transition text-left"
                  >
                    <Download className="h-4 w-4" />
                    {opt.label}
                  </button>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
