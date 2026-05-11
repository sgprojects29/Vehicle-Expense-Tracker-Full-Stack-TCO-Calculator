import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip } from 'recharts';
import { formatCurrency } from '../../utils/helpers';
import type { CategoryBreakdownItem, CostBreakdownDto } from '../../types/Report';

// Category-specific colors matching ExpensesPage badges
const getCategoryColor = (category: string): string => {
  switch (category) {
    case 'Maintenance':
      return '#3b82f6'; // Blue
    case 'Insurance':
      return '#a855f7'; // Purple
    case 'Registration':
      return '#6366f1'; // Indigo
    case 'Repairs':
      return '#ef4444'; // Red
    case 'Parking':
      return '#eab308'; // Yellow
    case 'Tolls':
      return '#f97316'; // Orange
    case 'Fuel & Charging':
      return '#10b981'; // Green (for fuel entries)
    case 'Purchase Price':
      return '#06b6d4'; // Cyan
    case 'Other':
      return '#6b7280'; // Gray
    default:
      return '#6b7280'; // Gray fallback
  }
};

interface CostBreakdownChartProps {
  breakdown: CostBreakdownDto;
}

// Extended type for chart data that includes the name property Recharts expects
interface ChartDataItem extends CategoryBreakdownItem {
  name: string;
  [key: string]: string | number; // Index signature for Recharts compatibility
}

export function CostBreakdownChart({ breakdown }: CostBreakdownChartProps) {
  if (breakdown.categoryBreakdown.length === 0) {
    return (
      <p className="text-gray-400 text-center py-8">No expense data available for breakdown</p>
    );
  }

  // Total Cost of Ownership - use backend data as-is (includes Purchase Price, Fuel & Charging, and all expenses)
  const tcoData: ChartDataItem[] = breakdown.categoryBreakdown
    .map(item => ({
      ...item,
      name: item.category,
    }))
    .filter(item => item.amount > 0); // Only show categories with costs

  // Operating Costs Only - exclude Purchase Price
  const operatingCostsData: ChartDataItem[] = breakdown.categoryBreakdown
    .filter(item => item.category !== 'Purchase Price')
    .map(item => {
      // Recalculate percentages based on operating costs total (without purchase price)
      const operatingTotal = breakdown.totalFuelCost + breakdown.totalExpensesCost;
      return {
        ...item,
        percentage: operatingTotal > 0 ? (item.amount / operatingTotal) * 100 : 0,
        name: item.category,
      };
    })
    .filter(item => item.amount > 0); // Only show categories with costs

  const operatingCostsTotal = breakdown.totalFuelCost + breakdown.totalExpensesCost;

  return (
    <div className="space-y-8">
      {/* Total Cost of Ownership Chart */}
      <div>
        <h3 className="text-lg font-semibold text-white mb-4">Total Cost of Ownership</h3>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Pie Chart */}
          <div className="h-80">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={tcoData}
                  dataKey="amount"
                  nameKey="category"
                  cx="50%"
                  cy="50%"
                  outerRadius={100}
                  label={(props: { payload?: ChartDataItem }) => {
                    const item = props.payload;
                    if (!item) return '';
                    // Only show label if percentage is >= 5% to avoid overlap
                    if (item.percentage < 5) return '';
                    return `${item.category} (${item.percentage.toFixed(1)}%)`;
                  }}
                  labelLine={false}
                >
                  {tcoData.map((item, index) => (
                    <Cell key={`cell-tco-${index}`} fill={getCategoryColor(item.category)} />
                  ))}
                </Pie>
                <Tooltip
                  formatter={(value: number | string | undefined) => {
                    if (value === undefined) return '$0.00';
                    return formatCurrency(Number(value));
                  }}
                  contentStyle={{ backgroundColor: '#1f2937', border: '1px solid #374151', borderRadius: '0.5rem' }}
                  labelStyle={{ color: '#f3f4f6' }}
                />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          </div>
          
          {/* Breakdown Table */}
          <div className="space-y-2">
            <div className="text-sm text-gray-400 mb-3">
              Total: <span className="text-white font-semibold">{formatCurrency(breakdown.totalCost)}</span>
            </div>
            {tcoData.map((item) => (
              <div
                key={`tco-${item.category}`}
                className="flex items-center justify-between p-3 bg-gray-700/50 rounded-lg"
              >
                <div className="flex items-center gap-3">
                  <div
                    className="w-4 h-4 rounded-full"
                    style={{ backgroundColor: getCategoryColor(item.category) }}
                  />
                  <span className="text-gray-200 font-medium">{item.category}</span>
                </div>
                <div className="text-right">
                  <div className="text-white font-semibold">{formatCurrency(item.amount)}</div>
                  <div className="text-gray-400 text-sm">
                    {item.percentage.toFixed(1)}%
                    {item.count > 0 && ` (${item.count} items)`}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Operating Costs Only Chart */}
      <div>
        <h3 className="text-lg font-semibold text-white mb-4">Operating Costs Breakdown</h3>
        <p className="text-sm text-gray-400 mb-4">Excludes purchase price - shows ongoing expenses only</p>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Pie Chart */}
          <div className="h-80">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={operatingCostsData}
                  dataKey="amount"
                  nameKey="category"
                  cx="50%"
                  cy="50%"
                  outerRadius={100}
                  label={(props: { payload?: ChartDataItem }) => {
                    const item = props.payload;
                    if (!item) return '';
                    // Only show label if percentage is >= 5% to avoid overlap
                    if (item.percentage < 5) return '';
                    return `${item.category} (${item.percentage.toFixed(1)}%)`;
                  }}
                  labelLine={false}
                >
                  {operatingCostsData.map((item, index) => (
                    <Cell key={`cell-operating-${index}`} fill={getCategoryColor(item.category)} />
                  ))}
                </Pie>
                <Tooltip
                  formatter={(value: number | string | undefined) => {
                    if (value === undefined) return '$0.00';
                    return formatCurrency(Number(value));
                  }}
                  contentStyle={{ backgroundColor: '#1f2937', border: '1px solid #374151', borderRadius: '0.5rem' }}
                  labelStyle={{ color: '#f3f4f6' }}
                />
                <Legend />
              </PieChart>
            </ResponsiveContainer>
          </div>
          
          {/* Breakdown Table */}
          <div className="space-y-2">
            <div className="text-sm text-gray-400 mb-3">
              Total: <span className="text-white font-semibold">{formatCurrency(operatingCostsTotal)}</span>
            </div>
            {operatingCostsData.map((item) => (
              <div
                key={`operating-${item.category}`}
                className="flex items-center justify-between p-3 bg-gray-700/50 rounded-lg"
              >
                <div className="flex items-center gap-3">
                  <div
                    className="w-4 h-4 rounded-full"
                    style={{ backgroundColor: getCategoryColor(item.category) }}
                  />
                  <span className="text-gray-200 font-medium">{item.category}</span>
                </div>
                <div className="text-right">
                  <div className="text-white font-semibold">{formatCurrency(item.amount)}</div>
                  <div className="text-gray-400 text-sm">
                    {item.percentage.toFixed(1)}%
                    {item.count > 0 && ` (${item.count} items)`}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
