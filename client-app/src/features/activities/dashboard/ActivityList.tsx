import { SyntheticEvent, useState } from "react";
import { Button, Item, Label, Segment } from "semantic-ui-react";
import { useStore } from "../../../app/stores/store";
import { observer } from "mobx-react-lite";
import { useNavigate } from "react-router-dom";

export default observer(function ActivityList() {
  const { activityStore } = useStore();
  const { deleteActivity, activities, loading } = activityStore;
  const [target, setTarget] = useState("");
  const navigate = useNavigate();

  function handleActivityDelete(e: SyntheticEvent<HTMLButtonElement>, id: string) {
    setTarget(e.currentTarget.name);
    deleteActivity(id);
  }

  return (
    <Segment>
      <Item.Group divided>
        {activities.map((activity) => (
          <Item key={activity.id} data-testid="activity-item">
            <Item.Content>
              <Item.Header as="a" data-testid="activity-title">{activity.title}</Item.Header>
              <Item.Meta>{activity.date}</Item.Meta>
              <Item.Description>
                <div>{activity.description}</div>
                <div>{activity.city}, {activity.venue}</div>
              </Item.Description>
              <Item.Extra>
                <Button
                  onClick={() => navigate(`/activities/${activity.id}`)}
                  floated="right"
                  content="View"
                  color="blue"
                  data-testid="view-button"
                />
                <Button
                  name={activity.id}
                  onClick={(e) => handleActivityDelete(e, activity.id)}
                  floated="right"
                  content="Delete"
                  color="red"
                  loading={loading && target === activity.id}
                  data-testid="delete-button"
                />
                <Label basic content={activity.category} />
              </Item.Extra>
            </Item.Content>
          </Item>
        ))}
      </Item.Group>
    </Segment>
  );
});
