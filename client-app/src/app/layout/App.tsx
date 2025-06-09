import { Fragment, useEffect } from "react";
import "./styles.css";
import { Container } from "semantic-ui-react";
import NavBar from "./NarBar";
import ActivityDashboard from "../../features/activities/dashboard/ActivityDashboard";
import LoadingComponent from "./LoadingComponents";
import { useStore } from "../stores/store";
import { observer } from "mobx-react-lite";

function App() {
  const { activityStore } = useStore();

  useEffect(() => {
    activityStore.loadActivities();
  }, [activityStore]);

  if (activityStore.loadingInitial)
    return <LoadingComponent content="Loading app"></LoadingComponent>;

  return (
    <Fragment>
      <NavBar></NavBar>
      <Container style={{ marginTop: "7em" }}>
        <ActivityDashboard></ActivityDashboard>
      </Container>
    </Fragment>
  );
}

export default observer(App);
