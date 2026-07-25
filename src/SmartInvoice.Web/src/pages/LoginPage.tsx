import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { Mail, Lock, Eye, EyeOff, Shield, Zap, Globe } from 'lucide-react';
import { useLoginMutation } from '../store/authApi';
import { useAppDispatch } from '../store/hooks';
import { setCredentials } from '../store/authSlice';

interface LoginForm {
  email: string;
  password: string;
}

export default function LoginPage() {
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const [login, { isLoading }] = useLoginMutation();
  const [showPassword, setShowPassword] = useState(false);

  const { register, handleSubmit, formState: { errors } } = useForm<LoginForm>();

  const onSubmit = async (data: LoginForm) => {
    try {
      const result = await login(data).unwrap();
      dispatch(setCredentials({
        accessToken: result.accessToken, refreshToken: result.refreshToken,
        userId: result.userId, email: result.email, fullName: result.fullName,
        companyId: result.companyId, roles: result.roles,
      }));
      toast.success('Welcome back!');
      navigate('/dashboard');
    } catch (err: unknown) {
      const error = err as { data?: { error?: string } };
      toast.error(error.data?.error || 'Login failed');
    }
  };

  return (
    <div className="flex min-h-screen h-screen overflow-hidden bg-[#0a0e1a]">
      {/* Left Panel */}
      <div className="hidden lg:flex lg:w-1/2 relative overflow-hidden p-10 flex-col justify-between">
        <div>
          <div className="flex items-center justify-center w-14 h-14 rounded-xl bg-blue-500/20 border border-blue-400/30 mb-6">
            <svg className="w-8 h-8 text-blue-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V9.375c0-.621-.504-1.125-1.125-1.125H8.25z" />
            </svg>
          </div>
          <h1 className="text-3xl font-bold text-white">Smart <span className="text-blue-400">Invoice</span></h1>
          <p className="text-slate-400 text-lg mt-4">Create. Manage. Get Paid.</p>
          <p className="text-slate-500 mt-2">Simple invoicing for<br />smarter businesses.</p>
        </div>

        {/* Invoice illustration */}
        <div className="flex-1 flex items-center justify-center min-h-0">
          <img src="/invoice-illustration.png" alt="Invoice illustration" className="max-h-[50vh] w-auto drop-shadow-2xl" />
        </div>

        {/* Bottom features */}
        <div className="flex gap-8 pb-2">
          <div className="flex items-center gap-2.5">
            <Shield className="h-4 w-4 text-blue-400" />
            <div><p className="text-white text-xs font-semibold">Secure</p><p className="text-slate-500 text-xs">Your data is protected</p></div>
          </div>
          <div className="flex items-center gap-2.5">
            <Zap className="h-4 w-4 text-blue-400" />
            <div><p className="text-white text-xs font-semibold">Fast</p><p className="text-slate-500 text-xs">Create invoices in seconds</p></div>
          </div>
          <div className="flex items-center gap-2.5">
            <Globe className="h-4 w-4 text-blue-400" />
            <div><p className="text-white text-xs font-semibold">Anywhere</p><p className="text-slate-500 text-xs">Access from any device</p></div>
          </div>
        </div>
      </div>

      {/* Right Panel - Form on dark bg */}
      <div className="flex-1 flex items-center justify-center p-8">
        <div className="w-full max-w-md">
          <h2 className="text-3xl font-bold text-white mb-1">Welcome back</h2>
          <p className="text-slate-400 mb-6">Sign in to your account</p>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-slate-400 mb-2">Email</label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none"><Mail className="h-5 w-5 text-slate-500" /></div>
                <input
                  type="email"
                  placeholder="you@example.com"
                  className={`w-full pl-12 pr-4 py-3.5 rounded-xl border ${errors.email ? 'border-red-500' : 'border-slate-700'} bg-[#131a2e] text-white text-sm placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all`}
                  {...register('email', { required: 'Email is required' })}
                />
              </div>
              {errors.email && <p className="mt-1 text-sm text-red-400">{errors.email.message}</p>}
            </div>

            <div>
              <label className="block text-sm font-medium text-slate-400 mb-2">Password</label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none"><Lock className="h-5 w-5 text-slate-500" /></div>
                <input
                  type={showPassword ? 'text' : 'password'}
                  placeholder="••••••••"
                  className={`w-full pl-12 pr-12 py-3.5 rounded-xl border ${errors.password ? 'border-red-500' : 'border-slate-700'} bg-[#131a2e] text-white text-sm placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all`}
                  {...register('password', { required: 'Password is required' })}
                />
                <button type="button" onClick={() => setShowPassword(!showPassword)} className="absolute inset-y-0 right-0 pr-4 flex items-center text-slate-500 hover:text-slate-300">
                  {showPassword ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
                </button>
              </div>
              {errors.password && <p className="mt-1 text-sm text-red-400">{errors.password.message}</p>}
            </div>

            <div className="flex items-center justify-between">
              <label className="flex items-center gap-2 cursor-pointer">
                <input type="checkbox" className="w-4 h-4 rounded border-slate-600 bg-[#131a2e] text-blue-500 focus:ring-blue-500 focus:ring-offset-0" />
                <span className="text-sm text-slate-400">Remember me</span>
              </label>
              <a href="#" className="text-sm text-blue-400 hover:text-blue-300 font-medium">Forgot password?</a>
            </div>

            <button
              type="submit"
              disabled={isLoading}
              className="w-full py-3.5 px-4 rounded-xl bg-gradient-to-r from-blue-500 via-blue-600 to-purple-600 text-white font-semibold text-base hover:from-blue-600 hover:via-blue-700 hover:to-purple-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 focus:ring-offset-[#0a0e1a] disabled:opacity-50 disabled:cursor-not-allowed transition-all shadow-lg shadow-blue-600/30"
            >
              {isLoading ? 'Signing in...' : 'Sign In'}
            </button>
          </form>

          <div className="relative my-6">
            <div className="absolute inset-0 flex items-center"><div className="w-full border-t border-slate-700/50" /></div>
            <div className="relative flex justify-center text-sm"><span className="px-4 bg-[#0a0e1a] text-slate-500">Or continue with</span></div>
          </div>

          <div className="flex justify-center gap-4">
            <button className="flex items-center justify-center w-12 h-12 rounded-full bg-[#131a2e] border border-slate-700 hover:border-slate-500 transition-colors">
              <svg className="w-5 h-5" viewBox="0 0 24 24"><path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/><path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/><path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/><path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/></svg>
            </button>
            <button className="flex items-center justify-center w-12 h-12 rounded-full bg-[#131a2e] border border-slate-700 hover:border-slate-500 transition-colors">
              <svg className="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 24 24"><path d="M12 0c-6.626 0-12 5.373-12 12 0 5.302 3.438 9.8 8.207 11.387.599.111.793-.261.793-.577v-2.234c-3.338.726-4.033-1.416-4.033-1.416-.546-1.387-1.333-1.756-1.333-1.756-1.089-.745.083-.729.083-.729 1.205.084 1.839 1.237 1.839 1.237 1.07 1.834 2.807 1.304 3.492.997.107-.775.418-1.305.762-1.604-2.665-.305-5.467-1.334-5.467-5.931 0-1.311.469-2.381 1.236-3.221-.124-.303-.535-1.524.117-3.176 0 0 1.008-.322 3.301 1.23.957-.266 1.983-.399 3.003-.404 1.02.005 2.047.138 3.006.404 2.291-1.552 3.297-1.23 3.297-1.23.653 1.653.242 2.874.118 3.176.77.84 1.235 1.911 1.235 3.221 0 4.609-2.807 5.624-5.479 5.921.43.372.823 1.102.823 2.222v3.293c0 .319.192.694.801.576 4.765-1.589 8.199-6.086 8.199-11.386 0-6.627-5.373-12-12-12z"/></svg>
            </button>
            <button className="flex items-center justify-center w-12 h-12 rounded-full bg-[#131a2e] border border-slate-700 hover:border-slate-500 transition-colors">
              <svg className="w-5 h-5" viewBox="0 0 24 24"><path fill="#F25022" d="M1 1h10v10H1z"/><path fill="#00A4EF" d="M1 13h10v10H1z"/><path fill="#7FBA00" d="M13 1h10v10H13z"/><path fill="#FFB900" d="M13 13h10v10H13z"/></svg>
            </button>
          </div>

          <p className="text-center mt-6 text-sm text-slate-500">
            Don&apos;t have an account?{' '}
            <Link to="/register" className="text-blue-400 hover:text-blue-300 font-semibold">Create one</Link>
          </p>
        </div>
      </div>
    </div>
  );
}
