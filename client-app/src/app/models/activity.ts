export interface Activity {
  id: string;
  title: string;
  date: string;
  description: string;
  category: string;
  city: string;
  venue: string;
}

export interface ActivityFormValues {
  title: string;
  date: string;
  description: string;
  category: string;
  city: string;
  venue: string;
}
