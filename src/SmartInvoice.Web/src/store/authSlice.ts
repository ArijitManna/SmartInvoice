import { createSlice, type PayloadAction } from '@reduxjs/toolkit';

export interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  userId: string | null;
  email: string | null;
  fullName: string | null;
  companyId: string | null;
  roles: string[];
  isAuthenticated: boolean;
}

const stored = localStorage.getItem('auth');
const initial: AuthState = stored
  ? JSON.parse(stored)
  : {
      accessToken: null,
      refreshToken: null,
      userId: null,
      email: null,
      fullName: null,
      companyId: null,
      roles: [],
      isAuthenticated: false,
    };

const authSlice = createSlice({
  name: 'auth',
  initialState: initial,
  reducers: {
    setCredentials(
      state,
      action: PayloadAction<{
        accessToken: string;
        refreshToken: string;
        userId: string;
        email: string;
        fullName: string;
        companyId: string | null;
        roles: string[];
      }>,
    ) {
      state.accessToken = action.payload.accessToken;
      state.refreshToken = action.payload.refreshToken;
      state.userId = action.payload.userId;
      state.email = action.payload.email;
      state.fullName = action.payload.fullName;
      state.companyId = action.payload.companyId;
      state.roles = action.payload.roles;
      state.isAuthenticated = true;
      localStorage.setItem('auth', JSON.stringify(state));
    },
    logout(state) {
      state.accessToken = null;
      state.refreshToken = null;
      state.userId = null;
      state.email = null;
      state.fullName = null;
      state.companyId = null;
      state.roles = [];
      state.isAuthenticated = false;
      localStorage.removeItem('auth');
    },
  },
});

export const { setCredentials, logout } = authSlice.actions;
export default authSlice.reducer;
