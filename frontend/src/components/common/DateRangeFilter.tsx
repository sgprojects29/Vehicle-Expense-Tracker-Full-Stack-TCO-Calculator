import { Calendar } from 'lucide-react';

interface DateRangeFilterProps {
  startDate: string;
  endDate: string;
  onStartDateChange: (date: string) => void;
  onEndDateChange: (date: string) => void;
  startDateLabel?: string;
  endDateLabel?: string;
}

export function DateRangeFilter({
  startDate,
  endDate,
  onStartDateChange,
  onEndDateChange,
  startDateLabel = 'Start Date',
  endDateLabel = 'End Date',
}: DateRangeFilterProps) {
  return (
    <>
      <div>
        <label htmlFor="filterStartDate" className="flex items-center text-sm font-medium text-gray-400 mb-1">
          <Calendar className="h-4 w-4 mr-1" />
          {startDateLabel}
        </label>
        <input
          type="date"
          id="filterStartDate"
          value={startDate}
          onChange={(e) => onStartDateChange(e.target.value)}
          className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>
      <div>
        <label htmlFor="filterEndDate" className="flex items-center text-sm font-medium text-gray-400 mb-1">
          <Calendar className="h-4 w-4 mr-1" />
          {endDateLabel}
        </label>
        <input
          type="date"
          id="filterEndDate"
          value={endDate}
          onChange={(e) => onEndDateChange(e.target.value)}
          className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded-md text-white focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>
    </>
  );
}
