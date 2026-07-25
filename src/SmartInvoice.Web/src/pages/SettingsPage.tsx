import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import { useNavigate } from 'react-router-dom';
import { Building2, MapPin, FileText, Landmark, Check } from 'lucide-react';
import { Button, Input, Card, CardContent } from '../components/ui';
import { stateNames, indianStates } from '../data/indianLocations';
import {
  useGetCurrentCompanyQuery,
  useCreateCompanyMutation,
  useUpdateCompanyMutation,
  type UpdateCompanyRequest,
} from '../store/companyApi';
import { useAppSelector } from '../store/hooks';

const steps = [
  { label: 'Company', icon: Building2 },
  { label: 'Address', icon: MapPin },
  { label: 'Tax Info', icon: FileText },
  { label: 'Bank', icon: Landmark },
];

export default function SettingsPage() {
  const navigate = useNavigate();
  const [currentStep, setCurrentStep] = useState(0);
  const { companyId } = useAppSelector((state) => state.auth);
  const { data: company, isLoading: loadingCompany, error } = useGetCurrentCompanyQuery(undefined, {
    skip: !companyId,
  });
  const [createCompany, { isLoading: creating }] = useCreateCompanyMutation();
  const [updateCompany, { isLoading: updating }] = useUpdateCompanyMutation();

  const hasCompany = !!company && !error;

  const { register, handleSubmit, reset, watch, setValue } = useForm<UpdateCompanyRequest>();
  const selectedState = watch('state');

  useEffect(() => {
    if (company) {
      reset({
        name: company.name,
        phone: company.phone ?? '',
        email: company.email ?? '',
        website: company.website ?? '',
        logoUrl: company.logoUrl ?? '',
        signatureUrl: company.signatureUrl ?? '',
        defaultCurrency: company.defaultCurrency,
        timeZone: company.timeZone,
        invoicePrefix: company.invoicePrefix ?? '',
        street: company.street ?? '',
        city: company.city ?? '',
        state: company.state ?? '',
        postalCode: company.postalCode ?? '',
        country: company.country ?? '',
        gstin: company.gstin ?? '',
        pan: company.pan ?? '',
        gstStateCode: company.gstStateCode ?? '',
        bankName: company.bankName ?? '',
        accountNumber: company.accountNumber ?? '',
        ifscCode: company.ifscCode ?? '',
        accountHolderName: company.accountHolderName ?? '',
        branchName: company.branchName ?? '',
        upiId: company.upiId ?? '',
      });
    }
  }, [company, reset]);

  const onSubmit = async (data: UpdateCompanyRequest) => {
    try {
      if (hasCompany) {
        await updateCompany(data).unwrap();
        toast.success('Settings saved successfully');
      } else {
        await createCompany(data).unwrap();
        toast.success('Company created! Please re-login for full access.');
        navigate('/login');
      }
    } catch {
      toast.error('Failed to save settings');
    }
  };

  const next = () => setCurrentStep((s) => Math.min(s + 1, steps.length - 1));
  const prev = () => setCurrentStep((s) => Math.max(s - 1, 0));
  const isLast = currentStep === steps.length - 1;

  if (loadingCompany && companyId) {
    return <div className="text-gray-500 dark:text-slate-400">Loading...</div>;
  }

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">
        {hasCompany ? 'Settings' : 'Create Company'}
      </h1>
      {!hasCompany && (
        <p className="mb-6 text-sm text-amber-700 dark:text-amber-300 bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-700/50 rounded-lg p-3">
          You haven't set up your company yet. Fill in the details below to get started.
        </p>
      )}

      {/* Step indicator */}
      <div className="flex items-center gap-1 mb-8">
        {steps.map((step, i) => (
          <div key={step.label} className="flex items-center">
            <button
              type="button"
              onClick={() => setCurrentStep(i)}
              className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
                i === currentStep
                  ? 'bg-blue-600 text-white'
                  : i < currentStep
                  ? 'bg-green-100 text-green-700 dark:bg-green-600/20 dark:text-green-400'
                  : 'bg-gray-100 text-gray-500 dark:bg-slate-800 dark:text-slate-400'
              }`}
            >
              {i < currentStep ? <Check className="h-4 w-4" /> : <step.icon className="h-4 w-4" />}
              {step.label}
            </button>
            {i < steps.length - 1 && (
              <div className={`w-8 h-0.5 mx-1 ${i < currentStep ? 'bg-green-300 dark:bg-green-600' : 'bg-gray-200 dark:bg-slate-700'}`} />
            )}
          </div>
        ))}
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="max-w-2xl">
        {/* Step 1: Company Info */}
        <Card className={currentStep === 0 ? '' : 'hidden'}>
          <CardContent className="p-6">
            <h2 className="text-lg font-semibold dark:text-white mb-4">Company Information</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input label="Company Name" {...register('name', { required: true })} />
              <Input label="Phone" {...register('phone')} />
              <Input label="Email" type="email" {...register('email')} />
              <Input label="Website" {...register('website')} />
              <Input label="Default Currency" {...register('defaultCurrency')} />
              <Input label="Time Zone" {...register('timeZone')} />
              <Input label="Invoice Prefix" {...register('invoicePrefix')} />
              <Input label="Logo URL" {...register('logoUrl')} />
              <Input label="Signature URL" {...register('signatureUrl')} />
            </div>
          </CardContent>
        </Card>

        {/* Step 2: Address */}
        <Card className={currentStep === 1 ? '' : 'hidden'}>
          <CardContent className="p-6">
            <h2 className="text-lg font-semibold dark:text-white mb-4">Business Address</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input label="Street" {...register('street')} />

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Country</label>
                <select
                  {...register('country')}
                  className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="India">India</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">State</label>
                <select
                  {...register('state')}
                  onChange={(e) => { setValue('state', e.target.value); setValue('city', ''); }}
                  className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">Select State</option>
                  {stateNames.map((s) => <option key={s} value={s}>{s}</option>)}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">City</label>
                <select
                  {...register('city')}
                  className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                >
                  <option value="">Select City</option>
                  {(selectedState && indianStates[selectedState] ? indianStates[selectedState] : []).map((c) => (
                    <option key={c} value={c}>{c}</option>
                  ))}
                </select>
              </div>

              <Input label="Postal Code" {...register('postalCode')} />
            </div>
          </CardContent>
        </Card>

        {/* Step 3: Tax Info */}
        <Card className={currentStep === 2 ? '' : 'hidden'}>
          <CardContent className="p-6">
            <h2 className="text-lg font-semibold dark:text-white mb-4">Tax Information</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input label="GSTIN" {...register('gstin')} />
              <Input label="PAN" {...register('pan')} />
              <Input label="GST State Code" {...register('gstStateCode')} />
            </div>
          </CardContent>
        </Card>

        {/* Step 4: Bank Details */}
        <Card className={currentStep === 3 ? '' : 'hidden'}>
          <CardContent className="p-6">
            <h2 className="text-lg font-semibold dark:text-white mb-4">Bank Details</h2>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input label="Bank Name" {...register('bankName')} />
              <Input label="Account Number" {...register('accountNumber')} />
              <Input label="IFSC Code" {...register('ifscCode')} />
              <Input label="Account Holder Name" {...register('accountHolderName')} />
              <Input label="Branch Name" {...register('branchName')} />
              <Input label="UPI ID" {...register('upiId')} />
            </div>
          </CardContent>
        </Card>

        {/* Navigation */}
        <div className="mt-6 flex justify-between">
          <Button type="button" variant="ghost" onClick={prev} disabled={currentStep === 0}>
            Previous
          </Button>
          <div className="flex gap-3">
            {!isLast && (
              <Button type="button" onClick={next}>
                Next
              </Button>
            )}
            {isLast && (
              <Button type="submit" loading={creating || updating}>
                {hasCompany ? 'Save Settings' : 'Create Company'}
              </Button>
            )}
          </div>
        </div>
      </form>
    </div>
  );
}
