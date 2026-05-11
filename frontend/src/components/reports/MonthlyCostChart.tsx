import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import { formatCurrency } from '../../utils/helpers';
import type { MonthlyDataPoint } from '../../types/Report';

interface MonthlyCostChartProps {
  monthlyData: MonthlyDataPoint[];
}

export function MonthlyCostChart({ monthlyData }: MonthlyCostChartProps) {
  if (monthlyData.length === 0) {
    return (
      <p className="text-gray-400 text-center py-8">No trend data available</p>
    );
  }

  return (
    <div className="h-80">
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={monthlyData}>
          <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
          <XAxis
            dataKey="monthName"
            stroke="#9ca3af"
            style={{ fontSize: '0.875rem' }}
          />
          <YAxis
            stroke="#9ca3af"
            style={{ fontSize: '0.875rem' }}
            tickFormatter={(value) => `$${value}`}
          />
          <Tooltip
            formatter={(value: number | undefined) => {
              if (value === undefined) return '$0.00';
              return formatCurrency(value);
            }}
            contentStyle={{ backgroundColor: '#1f2937', border: '1px solid #374151', borderRadius: '0.5rem' }}
            labelStyle={{ color: '#f3f4f6' }}
          />
          <Legend />
          <Line
            type="monotone"
            dataKey="fuelCost"
            stroke="#f59e0b"
            strokeWidth={2}
            name="Fuel Cost"
            dot={{ fill: '#f59e0b' }}
          />
          <Line
            type="monotone"
            dataKey="expensesCost"
            stroke="#8b5cf6"
            strokeWidth={2}
            name="Expenses"
            dot={{ fill: '#8b5cf6' }}
          />
          <Line
            type="monotone"
            dataKey="totalCost"
            stroke="#3b82f6"
            strokeWidth={2}
            name="Total Cost"
            dot={{ fill: '#3b82f6' }}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
