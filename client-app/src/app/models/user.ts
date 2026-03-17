export interface User {
  displayName: string;
  username: string;
  token: string;
}

export interface UserFormValues {
  email: string;
  password: string;
  displayName?: string;
  username?: string;
}
