import { useState } from 'react';
import type { ReactNode, LoginDto, RegisterDto } from '../types';
import { authService } from '../services/authService';
import { AuthContext } from './AuthContext';

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<{ email: string } | null>(() => {
    return authService.getStoredUser();
  });

  const login = async (data: LoginDto) => {
    const response = await authService.login(data);
    setUser({ email: response.email });
  };

  const register = async (data: RegisterDto) => {
    const response = await authService.register(data);
    setUser({ email: response.email });
  };

  const logout = () => {
    authService.logout();
    setUser(null);
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        login,
        register,
        logout,
        isAuthenticated: !!user,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}
