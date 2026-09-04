import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import { Building2, MapPin, FileText, Landmark, Check } from 'lucide-react';
import { Button, Input } from '../components/ui';
import { useCreateCompanyMutation, type CreateCompanyRequest } from '../store/companyApi';

const steps = [
  { label: 'Company', icon: Building2 },
  { label: 'Address', icon: MapPin },
  { label: 'GST / PAN', icon: FileText },
  { label: 'Bank', icon: Landmark },
];

export default function OnboardingPage() {
  const [currentStep, setCurrentStep] = useState(0);
  const navigate = useNavigate();
  const [createCompany, { isLoading }] = useCreateCompanyMutation();

  const { register, handleSubmit, formState: { errors } } = useForm<CreateCompanyRequest>();

  const onSubmit = async (data: CreateCompanyRequest) => {
    try {
      const result = await createCompany(data).unwrap();
      
      // If API returned a new token with CompanyId, store it and navigate to dashboard
      if (result.accessToken) {
        localStorage.setItem('token', result.accessToken);
        localStorage.setItem('user', JSON.stringify({
          id: result.userId,
          email: result.email,
          fullName: result.fullName,
          companyId: result.companyId,
          roles: result.roles
        }));
        toast.success('Company created successfully!');
        navigate('/dashboard');
      } else {
        toast.success('Company created! Please re-login for full access.');
        navigate('/login');
      }
    } catch (err: unknown) {
      const error = err as { data?: { error?: string } };
      toast.error(error.data?.error || 'Failed to create company');
    }
  };

  const next = () => setCurrentStep((s) => Math.min(s + 1, steps.length - 1));
  const prev = () => setCurrentStep((s) => Math.max(s - 1, 0));
  const isLast = currentStep === steps.length - 1;

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
      <div className="w-full max-w-lg">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-bold text-gray-900">Set up your business</h1>
          <p className="mt-2 text-sm text-gray-600">Tell us about your company to get started.</p>
        </div>

        {/* Step indicator */}
        <div className="mb-8 flex items-center justify-center gap-2">
          {steps.map((step, i) => (
            <div key={step.label} className="flex items-center">
              <div className={`flex h-8 w-8 items-center justify-center rounded-full text-sm font-medium ${
                i < currentStep ? 'bg-green-100 text-green-700' :
                i === currentStep ? 'bg-blue-600 text-white' :
                'bg-gray-100 text-gray-400'
              }`}>
                {i < currentStep ? <Check className="h-4 w-4" /> : i + 1}
              </div>
              {i < steps.length - 1 && (
                <div className={`mx-2 h-0.5 w-8 ${i < currentStep ? 'bg-green-300' : 'bg-gray-200'}`} />
              )}
            </div>
          ))}
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
          {/* Step 1: Company Info */}
          <div className={currentStep === 0 ? '' : 'hidden'}>
            <h2 className="mb-4 text-lg font-semibold text-gray-900">Company Details</h2>
            <div className="space-y-4">
              <Input label="Company Name" error={errors.name?.message}
                {...register('name', { required: 'Company name is required' })} />
              <Input label="Phone" {...register('phone')} />
              <Input label="Email" type="email" {...register('email')} />
              <Input label="Website" {...register('website')} placeholder="https://" />
            </div>
          </div>

          {/* Step 2: Address */}
          <div className={currentStep === 1 ? '' : 'hidden'}>
            <h2 className="mb-4 text-lg font-semibold text-gray-900">Business Address</h2>
            <div className="space-y-4">
              <Input label="Street" {...register('street')} />
              <div className="grid grid-cols-2 gap-4">
                <Input label="City" {...register('city')} />
                <Input label="State" {...register('state')} />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <Input label="Postal Code" {...register('postalCode')} />
                <Input label="Country" {...register('country')} defaultValue="India" />
              </div>
            </div>
          </div>

          {/* Step 3: GST/PAN */}
          <div className={currentStep === 2 ? '' : 'hidden'}>
            <h2 className="mb-4 text-lg font-semibold text-gray-900">Tax Information</h2>
            <div className="space-y-4">
              <Input label="GSTIN" {...register('gstin')} placeholder="e.g. 29ABCDE1234F1Z5" />
              <Input label="PAN" {...register('pan')} placeholder="e.g. ABCDE1234F" />
              <Input label="GST State Code" {...register('gstStateCode')} placeholder="e.g. 29" />
              <Input label="Invoice Prefix" {...register('invoicePrefix')} defaultValue="INV" />
            </div>
          </div>

          {/* Step 4: Bank Details */}
          <div className={currentStep === 3 ? '' : 'hidden'}>
            <h2 className="mb-4 text-lg font-semibold text-gray-900">Bank Details</h2>
            <div className="space-y-4">
              <Input label="Bank Name" {...register('bankName')} />
              <Input label="Account Number" {...register('accountNumber')} />
              <Input label="IFSC Code" {...register('ifscCode')} />
              <Input label="Account Holder Name" {...register('accountHolderName')} />
              <Input label="UPI ID" {...register('upiId')} placeholder="e.g. business@upi" />
            </div>
          </div>

          {/* Navigation */}
          <div className="mt-6 flex justify-between">
            <Button type="button" variant="ghost" onClick={prev} disabled={currentStep === 0}>
              Previous
            </Button>
            {isLast ? (
              <Button type="submit" loading={isLoading}>
                Create Company
              </Button>
            ) : (
              <Button type="button" onClick={next}>
                Next
              </Button>
            )}
          </div>
        </form>
      </div>
    </div>
  );
}
