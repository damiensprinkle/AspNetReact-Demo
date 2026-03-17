import { useEffect } from "react";
import "./styles.css";
import { Container } from "semantic-ui-react";
import NavBar from "./NavBar";
import LoadingComponent from "./LoadingComponents";
import { useStore } from "../stores/store";
import { observer } from "mobx-react-lite";
import { Outlet } from "react-router-dom";

function App() {
  const { activityStore, userStore } = useStore();

  useEffect(() => {
    const token = localStorage.getItem("jwt");
    if (token) userStore.getUser();
    activityStore.loadActivities();
  }, [activityStore, userStore]);

  if (activityStore.loadingInitial)
    return <LoadingComponent content="Loading app" />;

  return (
    <>
      <NavBar />
      <Container style={{ marginTop: "7em" }}>
        <Outlet />
      </Container>
    </>
  );
}

export default observer(App);
