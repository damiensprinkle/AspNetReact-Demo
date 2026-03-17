import {
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardMeta,
  Image,
} from "semantic-ui-react";
import { useStore } from "../../../app/stores/store";
import LoadingComponent from "../../../app/layout/LoadingComponents";
import { observer } from "mobx-react-lite";
import { useNavigate, useParams } from "react-router-dom";
import { useEffect } from "react";

export default observer(function ActivityDetails() {
  const { activityStore } = useStore();
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  useEffect(() => {
    if (id) activityStore.selectActivity(id);
    return () => activityStore.cancelSelectedActivity();
  }, [id, activityStore]);

  const activity = activityStore.selectedActivity;

  if (!activity) return <LoadingComponent />;

  return (
    <Card fluid>
      <Image src={`/assets/categoryImages/${activity.category}.jpg`} />
      <CardContent>
        <CardHeader data-testid="activity-title">{activity.title}</CardHeader>
        <CardMeta>
          <span data-testid="activity-date">{activity.date}</span>
        </CardMeta>
        <CardDescription data-testid="activity-description">{activity.description}</CardDescription>
      </CardContent>
      <CardContent extra>
        <Button.Group widths="2">
          <Button
            onClick={() => navigate(`/activities/${activity.id}/edit`)}
            basic
            color="blue"
            content="Edit"
            data-testid="edit-button"
          />
          <Button
            onClick={() => navigate("/activities")}
            basic
            color="grey"
            content="Cancel"
            data-testid="cancel-button"
          />
        </Button.Group>
      </CardContent>
    </Card>
  );
});
