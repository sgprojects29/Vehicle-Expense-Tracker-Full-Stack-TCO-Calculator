import { createContext } from 'react';
import type { LoginDto, RegisterDto } from '../types';

export interface AuthContextType {
  user: { email: string } | null;
  login: (data: LoginDto) => Promise<void>;
  register: (data: RegisterDto) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined);
