import { useState, useEffect } from 'react';
import { Wallet,
  Plus,
  Edit,
  Trash2,
  Loader2,
  AlertCircle,
  Filter,
  X } from 'lucide-react';
import { Navigation } from '../components/Navigation';
import { ExpenseForm } from '../components/expense/ExpenseForm';
import { DeleteExpenseModal } from '../components/expense/DeleteExpenseModal';
import { Pagination } from '../components/common/Pagination';
import { DateRangeFilter } from '../components/common/DateRangeFilter';
import { expenseService } from '../services/expenseService';
import { vehicleService } from '../services/vehicleService';
import { formatCurrency, formatDateOnly } from '../utils/helpers';
import type { Expense, CreateExpenseDto, UpdateExpenseDto } from '../types/Expense';
import type { Vehicle } from '../types/Vehicle';
import { Link } from 'react-router-dom';

export function ExpensesPage() {
  const [expenses, setExpenses] = useState<Expense[]>([]);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  const [showForm, setShowForm] = useState(false);
  const [editingExpense, setEditingExpense] = useState<Expense | undefined>(undefined);
  
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [deletingExpense, setDeletingExpense] = useState<Expense | undefined>(undefined);

  // Filters
  const [selectedVehicleId, setSelectedVehicleId] = useState<number | undefined>(undefined);
  const [selectedCategory, setSelectedCategory] = useState<number | undefined>(undefined);

  // Date filters
  const [startDate, setStartDate] = useState<string>('');
  const [endDate, setEndDate] = useState<string>('');

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(15); // Now configurable

  const categoryOptions = [
    // { value: 0, label: 'Fuel' }, // REMOVED - Use Fuel page instead
    { value: 1, label: 'Maintenance' },
    { value: 2, label: 'Insurance' },
    { value: 3, label: 'Registration' },
    { value: 4, label: 'Repairs' },
    { value: 5, label: 'Parking' },
    { value: 6, label: 'Tolls' },
    { value: 7, label: 'Car Wash' },
    { value: 8, label: 'Modifications' },
    { value: 9, label: 'Other' },
  ];

  useEffect(() => {
    fetchInitialData();
  }, []);

  useEffect(() => {
    if (vehicles.length > 0) {
      fetchExpenses();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedVehicleId, selectedCategory, startDate, endDate]);

  const fetchInitialData = async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      // Fetch vehicles first
      const vehiclesData = await vehicleService.getAll();
      setVehicles(vehiclesData);
      
      // Then fetch expenses
      const expensesData = await expenseService.getAll();
      setExpenses(expensesData);
    } catch (err) {
      console.error('Error fetching data:', err);
      setError('Failed to load data. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const fetchExpenses = async () => {
    try {
      setError(null);
      const data = await expenseService.getAll(
        selectedVehicleId, 
        selectedCategory,
        startDate || undefined,
        endDate || undefined
      );
      setExpenses(data);
      setCurrentPage(1); // Reset to first page when filters change
    } catch (err) {
      console.error('Error fetching expenses:', err);
      setError('Failed to load expenses. Please try again.');
    }
  };

  const handleAddClick = () => {
    setEditingExpense(undefined);
    setShowForm(true);
  };

  const handleEditClick = (expense: Expense) => {
    setEditingExpense(expense);
    setShowForm(true);
  };

  const handleDeleteClick = (expense: Expense) => {
    setDeletingExpense(expense);
    setShowDeleteModal(true);
  };

  const handleFormSubmit = async (data: CreateExpenseDto | UpdateExpenseDto) => {
    if (editingExpense) {
      await expenseService.update(editingExpense.id, data as UpdateExpenseDto);
    } else {
      await expenseService.create(data as CreateExpenseDto);
    }
    
    setShowForm(false);
    setEditingExpense(undefined);
    await fetchExpenses();
  };

  const handleFormCancel = () => {
    setShowForm(false);
    setEditingExpense(undefined);
  };

  const handleDeleteConfirm = async () => {
    if (!deletingExpense) return;
    
    await expenseService.delete(deletingExpense.id);
    
    setShowDeleteModal(false);
    setDeletingExpense(undefined);
    await fetchExpenses();
  };

  const handleDeleteCancel = () => {
    setShowDeleteModal(false);
    setDeletingExpense(undefined);
  };

  const clearFilters = () => {
    setSelectedVehicleId(undefined);
    setSelectedCategory(undefined);
    setStartDate('');
    setEndDate('');
    setCurrentPage(1);
  };

  const hasActiveFilters = selectedVehicleId !== undefined || selectedCategory !== undefined || startDate !== '' || endDate !== '';

  const getCategoryBadgeColor = (category: number): string => {
    switch (category) {
      case 1: // Maintenance
        return 'bg-blue-900/50 text-blue-300 border-blue-700';
      case 2: // Insurance
        return 'bg-purple-900/50 text-purple-300 border-purple-700';
      case 3: // Registration
        return 'bg-indigo-900/50 text-indigo-300 border-indigo-700';
      case 4: // Repairs
        return 'bg-red-900/50 text-red-300 border-red-700';
      case 5: // Parking
        return 'bg-yellow-900/50 text-yellow-300 border-yellow-700';
      case 6: // Tolls
        return 'bg-orange-900/50 text-orange-300 border-orange-700';
      case 7: // Other
        return 'bg-gray-900/50 text-gray-300 border-gray-700';
      default:
        return 'bg-gray-900/50 text-gray-300 border-gray-700';
    }
  };

  const sortedExpenses = [...expenses].sort((a, b) => {
    return new Date(b.date).getTime() - new Date(a.date).getTime();
  });

  // Pagination calculations
  const totalExpenses = sortedExpenses.length;
  const totalPages = Math.ceil(totalExpenses / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  const paginatedExpenses = sortedExpenses.slice(startIndex, endIndex);


  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-900">
        <Navigation />
        <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="flex justify-center items-center h-64">
            <div className="flex items-center space-x-2">
              <Loader2 className="h-8 w-8 text-blue-500 animate-spin" />
              <span className="text-gray-400 text-lg">Loading expenses...</span>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gray-900">
        <Navigation />
        <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
          <div className="bg-red-900/50 border border-red-700 rounded-lg p-4">
            <div className="flex">
              <div className="shrink-0">
                <AlertCircle className="h-5 w-5 text-red-300" />
              </div>
              <div className="ml-3">
                <p className="text-sm font-medium text-red-200">{error}</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-900">
      <Navigation />

      <div className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="flex justify-between items-center">
            <div>
              <h1 className="text-3xl font-bold text-white">Expenses</h1>
              <p className="mt-2 text-sm text-gray-400">
                Track and categorize all your vehicle expenses
              </p>
              <p className="text-sm text-gray-400 mb-4">
                To log fuel costs, use the <Link to="/fuel" className="text-blue-400 hover:underline">Fuel page</Link>
              </p>
            </div>
            <button
              onClick={handleAddClick}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
              disabled={vehicles.length === 0}
            >
              <Plus className="h-5 w-5 mr-2" />
              Add Expense
            </button>
          </div>
        </div>

        <div className="px-4 sm:px-0 mt-6">
          <div className="bg-gray-800 rounded-lg border border-gray-700 p-4">
            <div className="flex items-center justify-between mb-3">
              <div className="flex items-center space-x-2">
                <Filter className="h-5 w-5 text-gray-400" />
                <h3 className="text-sm font-medium text-gray-300">Filters</h3>
              </div>
              {hasActiveFilters && (
                <button
                  onClick={clearFilters}
                  className="inline-flex items-center text-sm text-blue-400 hover:text-blue-300 transition-colors"
                >
                  <X className="h-4 w-4 mr-1" />
                  Clear Filters
                </button>
              )}
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
              <div>
                <label htmlFor="filterVehicle" className="block text-sm font-medium text-gray-400 mb-1">
                  Vehicle
                </label>
                <select
                  id="filterVehicle"
                  value={selectedVehicleId ?? ''}
                  onChange={(e) => setSelectedVehicleId(e.target.value ? parseInt(e.target.value) : undefined)}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">All Vehicles</option>
                  {vehicles.map(vehicle => (
                    <option key={vehicle.id} value={vehicle.id}>
                      {vehicle.year} {vehicle.make} {vehicle.model}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label htmlFor="filterCategory" className="block text-sm font-medium text-gray-400 mb-1">
                  Category
                </label>
                <select
                  id="filterCategory"
                  value={selectedCategory ?? ''}
                  onChange={(e) => setSelectedCategory(e.target.value ? parseInt(e.target.value) : undefined)}
                  className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                >
                  <option value="">All Categories</option>
                  {categoryOptions.map(option => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </div>
              <DateRangeFilter
                startDate={startDate}
                endDate={endDate}
                onStartDateChange={setStartDate}
                onEndDateChange={setEndDate}
              />
            </div>
          </div>
        </div>

        <div className="px-4 sm:px-0 mt-6">
          {vehicles.length === 0 ? (
            <div className="bg-gray-800 shadow-lg rounded-lg border border-gray-700">
              <div className="text-center py-12">
                <Wallet className="mx-auto h-12 w-12 text-gray-600" />
                <h3 className="mt-2 text-sm font-medium text-gray-300">No vehicles yet</h3>
                <p className="mt-1 text-sm text-gray-400">
                  You need to add a vehicle before you can track expenses.
                </p>
              </div>
            </div>
          ) : sortedExpenses.length === 0 ? (
            <div className="bg-gray-800 shadow-lg rounded-lg border border-gray-700">
              <div className="text-center py-12">
                <Wallet className="mx-auto h-12 w-12 text-gray-600" />
                <h3 className="mt-2 text-sm font-medium text-gray-300">
                  {hasActiveFilters ? 'No expenses match your filters' : 'No expenses'}
                </h3>
                <p className="mt-1 text-sm text-gray-400">
                  {hasActiveFilters 
                    ? 'Try adjusting your filters to see more expenses.'
                    : 'Get started by adding your first expense.'}
                </p>
                {!hasActiveFilters && (
                  <div className="mt-6">
                    <button
                      onClick={handleAddClick}
                      className="inline-flex items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
                    >
                      <Plus className="h-4 w-4 mr-2" />
                      Add Expense
                    </button>
                  </div>
                )}
              </div>
            </div>
          ) : (
            <div className="bg-gray-800 shadow-lg rounded-lg border border-gray-700 overflow-hidden">
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-700">
                  <thead className="bg-gray-900">
                    <tr>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                        Date
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                        Vehicle
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                        Category
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                        Amount
                      </th>
                      <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-400 uppercase tracking-wider">
                        Notes
                      </th>
                      <th scope="col" className="px-6 py-3 text-right text-xs font-medium text-gray-400 uppercase tracking-wider">
                        Actions
                      </th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-700">
                    {paginatedExpenses.map((expense) => (
                      <tr key={expense.id} className="hover:bg-gray-750 transition-colors">
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-300">
                          {formatDateOnly(expense.date)}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm font-medium text-white">
                            {expense.vehicleMake} {expense.vehicleModel}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${getCategoryBadgeColor(expense.category)}`}>
                            {expense.categoryName}
                          </span>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-white">
                          {formatCurrency(expense.amount)}
                        </td>
                        <td className="px-6 py-4 text-sm text-gray-400">
                          <div className="max-w-xs truncate">
                            {expense.notes || '-'}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                          <button
                            onClick={() => handleEditClick(expense)}
                            className="text-blue-400 hover:text-blue-300 mr-3 transition-colors"
                          >
                            <Edit className="h-4 w-4 inline" />
                          </button>
                          <button
                            onClick={() => handleDeleteClick(expense)}
                            className="text-red-400 hover:text-red-300 transition-colors"
                          >
                            <Trash2 className="h-4 w-4 inline" />
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <Pagination
                currentPage={currentPage}
                totalPages={totalPages}
                totalItems={totalExpenses}
                itemsPerPage={itemsPerPage}
                onPageChange={(page) => {
                  setCurrentPage(page);
                  window.scrollTo({ top: 0, behavior: 'smooth' });
                }}
                onItemsPerPageChange={(newItemsPerPage) => {
                  setItemsPerPage(newItemsPerPage);
                  setCurrentPage(1);
                }}
                itemLabel="expense"
                itemLabelPlural="expenses"
              />
            </div>
          )}
        </div>
      </div>

      {showForm && (
        <ExpenseForm
          expense={editingExpense}
          vehicles={vehicles}
          onSubmit={handleFormSubmit}
          onCancel={handleFormCancel}
        />
      )}

      {showDeleteModal && deletingExpense && (
        <DeleteExpenseModal
          expense={deletingExpense}
          onConfirm={handleDeleteConfirm}
          onCancel={handleDeleteCancel}
        />
      )}
    </div>
  );
}
