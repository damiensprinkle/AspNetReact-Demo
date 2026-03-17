import { Button, Container, Menu } from "semantic-ui-react";
import { NavLink, useNavigate } from "react-router-dom";
import { useStore } from "../stores/store";
import { observer } from "mobx-react-lite";

export default observer(function NavBar() {
  const { userStore } = useStore();
  const navigate = useNavigate();

  return (
    <Menu inverted fixed="top">
      <Container>
        <Menu.Item header as={NavLink} to="/">
          <img src="/assets/logo.png" alt="logo" style={{ marginRight: "10px" }} />
          Activities
        </Menu.Item>

        {userStore.isLoggedIn ? (
          <>
            <Menu.Item name="Activities" as={NavLink} to="/activities" data-testid="activities-link" />
            <Menu.Item>
              <Button as={NavLink} to="/activities/create" positive content="Create Activity" data-testid="create-activity-button" />
            </Menu.Item>
            <Menu.Item position="right">
              <span style={{ marginRight: "10px" }}>Hi, {userStore.user?.displayName}</span>
              <Button
                content="Logout"
                onClick={() => { userStore.logout(); navigate("/"); }}
                data-testid="logout-button"
              />
            </Menu.Item>
          </>
        ) : (
          <Menu.Item position="right">
            <Button as={NavLink} to="/login" content="Login" style={{ marginRight: "5px" }} data-testid="login-button" />
            <Button as={NavLink} to="/register" positive content="Register" data-testid="register-button" />
          </Menu.Item>
        )}
      </Container>
    </Menu>
  );
});
