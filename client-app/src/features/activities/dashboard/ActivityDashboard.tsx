import { Grid } from "semantic-ui-react";
import ActivityList from "./ActivityList";
import { observer } from "mobx-react-lite";

export default observer(function ActivityDashboard() {
  return (
    <Grid>
      <Grid.Column width="16">
        <ActivityList />
      </Grid.Column>
    </Grid>
  );
});
