import axios from 'axios';

// In production, VITE_API_URL is baked in at build time via fly.toml
// In local Docker, VITE_API_URL='http://backend:8080' which triggers proxy usage
// The dev server proxy (vite.config.ts) forwards /api to backend
const envApiUrl = import.meta.env.VITE_API_URL;
const API_BASE_URL = envApiUrl?.startsWith('http://backend')
  ? '/api'  // Docker dev: use relative path, proxy forwards to backend:8080
  : envApiUrl || '/api';  // Production: use full URL from build args, or fallback to relative

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add auth token to requests
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Handle 401 errors (redirect to login)
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    // Only redirect on 401 if we're NOT on the login/register endpoints
    if (error.response?.status === 401) {
      const isAuthEndpoint = error.config?.url?.includes('/auth/');
      
      if (!isAuthEndpoint) {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export default apiClient;
