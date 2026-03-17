import { makeAutoObservable, runInAction } from "mobx";
import { Activity, ActivityFormValues } from "../models/activity";
import agent from "../api/agent";

export default class ActivityStore {
  activities: Activity[] = [];
  selectedActivity: Activity | undefined = undefined;
  loading = false;
  loadingInitial = false;

  constructor() {
    makeAutoObservable(this);
  }

  loadActivities = async () => {
    this.setLoadingInitial(true);
    try {
      const activities = await agent.activities.list();
      runInAction(() => {
        this.activities = activities.map((a) => ({
          ...a,
          date: a.date.split("T")[0],
        }));
      });
    } catch (error) {
      console.error(error);
    } finally {
      this.setLoadingInitial(false);
    }
  };

  setLoadingInitial = (state: boolean) => {
    this.loadingInitial = state;
  };

  selectActivity = (id?: string) => {
    this.selectedActivity = this.activities.find((a) => a.id === id);
  };

  cancelSelectedActivity = () => {
    this.selectedActivity = undefined;
  };

  createActivityWithReturn = async (values: ActivityFormValues): Promise<Activity | undefined> => {
    this.loading = true;
    try {
      const activity = await agent.activities.create(values);
      runInAction(() => {
        this.activities.push(activity);
        this.selectedActivity = activity;
      });
      return activity;
    } catch (error) {
      console.error(error);
      return undefined;
    } finally {
      runInAction(() => { this.loading = false; });
    }
  };

  updateActivity = async (id: string, values: ActivityFormValues) => {
    this.loading = true;
    try {
      const updated = await agent.activities.update(id, values);
      runInAction(() => {
        this.activities = this.activities.map((a) => (a.id === id ? updated : a));
        this.selectedActivity = updated;
      });
    } catch (error) {
      console.error(error);
    } finally {
      runInAction(() => { this.loading = false; });
    }
  };

  deleteActivity = async (id: string) => {
    this.loading = true;
    try {
      await agent.activities.delete(id);
      runInAction(() => {
        this.activities = this.activities.filter((a) => a.id !== id);
        if (this.selectedActivity?.id === id) this.cancelSelectedActivity();
      });
    } catch (error) {
      console.error(error);
    } finally {
      runInAction(() => { this.loading = false; });
    }
  };
}
