import { Button, Form, Header, Segment } from "semantic-ui-react";
import { ChangeEvent, useState } from "react";
import { useStore } from "../../app/stores/store";
import { observer } from "mobx-react-lite";
import { useNavigate } from "react-router-dom";
import { UserFormValues } from "../../app/models/user";

export default observer(function LoginForm() {
  const { userStore } = useStore();
  const navigate = useNavigate();
  const [values, setValues] = useState<UserFormValues>({ email: "", password: "" });
  const [error, setError] = useState<string | null>(null);

  function handleChange(e: ChangeEvent<HTMLInputElement>) {
    setValues({ ...values, [e.target.name]: e.target.value });
  }

  async function handleSubmit() {
    setError(null);
    try {
      await userStore.login(values);
      navigate("/activities");
    } catch {
      setError("Invalid email or password");
    }
  }

  return (
    <Segment clearing style={{ maxWidth: 450, margin: "5em auto" }}>
      <Header as="h2" content="Log In" color="teal" textAlign="center" />
      <Form onSubmit={handleSubmit} autoComplete="off">
        <Form.Input placeholder="Email" name="email" value={values.email} onChange={handleChange} />
        <Form.Input placeholder="Password" name="password" type="password" value={values.password} onChange={handleChange} />
        {error && <p data-testid="login-error" style={{ color: "red" }}>{error}</p>}
        <Button fluid positive type="submit" content="Login" />
      </Form>
    </Segment>
  );
});
