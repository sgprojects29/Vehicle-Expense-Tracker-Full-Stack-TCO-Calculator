import { Link, useLocation } from 'react-router-dom';
import { LayoutDashboard, Car, Wallet, Fuel, BarChart3, LogOut } from 'lucide-react';
import { useAuth } from '../hooks/useAuth';

export function Navigation() {
  const location = useLocation();
  const { user, logout } = useAuth();

  const navLinks = [
    { name: 'Dashboard', path: '/', icon: LayoutDashboard },
    { name: 'Vehicles', path: '/vehicles', icon: Car },
    { name: 'Expenses', path: '/expenses', icon: Wallet },
    { name: 'Fuel', path: '/fuel', icon: Fuel },
    { name: 'Reports', path: '/reports', icon: BarChart3 },
  ];

  const isActive = (path: string) => {
    return location.pathname === path;
  };

  return (
    <nav className="bg-gray-800 border-b border-gray-700">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between h-16">
          <div className="flex">
            <div className="shrink-0 flex items-center">
              <Car className="h-6 w-6 text-blue-500 mr-2" />
              <h1 className="text-xl font-bold text-white">Vehicle Expense Tracker</h1>
            </div>
            <div className="hidden sm:ml-6 sm:flex sm:space-x-8">
              {navLinks.map((link) => {
                const Icon = link.icon;
                return (
                  <Link
                    key={link.path}
                    to={link.path}
                    className={`inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium ${
                      isActive(link.path)
                        ? 'border-blue-500 text-white'
                        : 'border-transparent text-gray-300 hover:border-gray-500 hover:text-white'
                    }`}
                  >
                    <Icon className="h-4 w-4 mr-2" />
                    {link.name}
                  </Link>
                );
              })}
            </div>
          </div>
          <div className="flex items-center">
            <span className="text-sm text-gray-400 mr-4">{user?.email}</span>
            <button
              onClick={logout}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
            >
              <LogOut className="h-4 w-4 mr-2" />
              Logout
            </button>
          </div>
        </div>
      </div>

      {/* Mobile menu */}
      <div className="sm:hidden">
        <div className="pt-2 pb-3 space-y-1">
          {navLinks.map((link) => {
            const Icon = link.icon;
            return (
              <Link
                key={link.path}
                to={link.path}
                className={`flex items-center pl-3 pr-4 py-2 border-l-4 text-base font-medium ${
                  isActive(link.path)
                    ? 'bg-gray-700 border-blue-500 text-white'
                    : 'border-transparent text-gray-300 hover:bg-gray-700 hover:border-gray-500 hover:text-white'
                }`}
              >
                <Icon className="h-5 w-5 mr-3" />
                {link.name}
              </Link>
            );
          })}
        </div>
      </div>
    </nav>
  );
}
