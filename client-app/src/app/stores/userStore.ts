import { makeAutoObservable, runInAction } from "mobx";
import { User, UserFormValues } from "../models/user";
import agent from "../api/agent";

export default class UserStore {
  user: User | null = null;

  constructor() {
    makeAutoObservable(this);
  }

  get isLoggedIn() {
    return !!this.user;
  }

  login = async (creds: UserFormValues) => {
    const user = await agent.account.login(creds);
    runInAction(() => {
      this.user = user;
      localStorage.setItem("jwt", user.token);
    });
  };

  register = async (creds: UserFormValues) => {
    const user = await agent.account.register(creds);
    runInAction(() => {
      this.user = user;
      localStorage.setItem("jwt", user.token);
    });
  };

  logout = () => {
    localStorage.removeItem("jwt");
    this.user = null;
  };

  getUser = async () => {
    try {
      const user = await agent.account.current();
      runInAction(() => { this.user = user; });
    } catch {
      // token invalid or expired — stay logged out
    }
  };
}
