import axios, { AxiosResponse } from "axios";
import { Activity, ActivityFormValues } from "../models/activity";
import { User, UserFormValues } from "../models/user";

axios.defaults.baseURL = "http://localhost:5000/api";

// Attach JWT token from localStorage on every request
axios.interceptors.request.use((config) => {
  const token = localStorage.getItem("jwt");
  if (token && config.headers) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

const responseBody = <T>(response: AxiosResponse<T>) => response.data;

const requests = {
  get: <T>(url: string) => axios.get<T>(url).then(responseBody),
  post: <T>(url: string, body: object) => axios.post<T>(url, body).then(responseBody),
  put: <T>(url: string, body: object) => axios.put<T>(url, body).then(responseBody),
  delete: <T>(url: string) => axios.delete<T>(url).then(responseBody),
};

const activities = {
  list: () => requests.get<Activity[]>("/activities"),
  details: (id: string) => requests.get<Activity>(`/activities/${id}`),
  create: (activity: ActivityFormValues) => requests.post<Activity>("/activities", activity),
  update: (id: string, activity: ActivityFormValues) => requests.put<Activity>(`/activities/${id}`, activity),
  delete: (id: string) => requests.delete<void>(`/activities/${id}`),
};

const account = {
  current: () => requests.get<User>("/account"),
  login: (user: UserFormValues) => requests.post<User>("/account/login", user),
  register: (user: UserFormValues) => requests.post<User>("/account/register", user),
};

const agent = {
  activities,
  account,
};

export default agent;
