export type EventApiModel = {
  id: string;
  title: string;
  description?: string | null;
  startDate: string;
  locationId: string;
  daysCount: number;
  percent: number;
  createdByUserId: string;
  rating: number;
  personCount: number;
};

export type EventListFilters = {
  titleContains?: string;
  pageNumber?: number;
  pageSize?: number;
};
