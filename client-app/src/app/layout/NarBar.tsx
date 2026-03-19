import { Button, Container, Menu } from "semantic-ui-react";
import { useNavigate } from "react-router-dom";

export default function NavBar() {
  const navigate = useNavigate();

  return (
    <Menu inverted fixed="top">
      <Container>
        <Menu.Item header>
          <img
            src="/assets/logo.png"
            alt="logo"
            style={{ marginRight: "10px" }}
          />
          Reactivities
        </Menu.Item>
        <Menu.Item name="Activities"></Menu.Item>
        <Menu.Item>
          <Button
            onClick={() => navigate("/activities/create")}
            positive
            content="Create Activity"
          ></Button>
        </Menu.Item>
      </Container>
    </Menu>
  );
}
