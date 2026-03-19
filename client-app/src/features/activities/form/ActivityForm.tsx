import { Button, Form, Segment } from "semantic-ui-react";
import { ChangeEvent, useEffect, useState } from "react";
import { useStore } from "../../../app/stores/store";
import { observer } from "mobx-react-lite";
import { ActivityFormValues } from "../../../app/models/activity";
import { useNavigate, useParams } from "react-router-dom";

export default observer(function ActivityForm() {
  const { activityStore } = useStore();
  const { updateActivity, loading, selectActivity, selectedActivity } = activityStore;
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [activity, setActivity] = useState<ActivityFormValues>({
    title: "",
    category: "",
    description: "",
    date: "",
    city: "",
    venue: "",
  });

  useEffect(() => {
    if (id) {
      selectActivity(id);
    }
  }, [id, selectActivity]);

  useEffect(() => {
    if (selectedActivity && id) {
      setActivity({
        title: selectedActivity.title,
        category: selectedActivity.category,
        description: selectedActivity.description,
        date: selectedActivity.date,
        city: selectedActivity.city,
        venue: selectedActivity.venue,
      });
    }
  }, [selectedActivity, id]);

  async function handleSubmit() {
    if (id && selectedActivity) {
      await updateActivity(selectedActivity.id, activity);
      navigate(`/activities/${selectedActivity.id}`);
    } else {
      const created = await activityStore.createActivityWithReturn(activity);
      if (created) navigate(`/activities/${created.id}`);
    }
  }

  function handleInputChange(event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) {
    const { name, value } = event.target;
    setActivity({ ...activity, [name]: value });
  }

  return (
    <Segment clearing>
      <Form onSubmit={handleSubmit} autoComplete="off">
        <Form.Input placeholder="Title" value={activity.title} name="title" onChange={handleInputChange} />
        <Form.TextArea placeholder="Description" value={activity.description} name="description" onChange={handleInputChange} />
        <Form.Input placeholder="Category" value={activity.category} name="category" onChange={handleInputChange} />
        <Form.Input type="date" placeholder="Date" value={activity.date} name="date" onChange={handleInputChange} />
        <Form.Input placeholder="City" value={activity.city} name="city" onChange={handleInputChange} />
        <Form.Input placeholder="Venue" value={activity.venue} name="venue" onChange={handleInputChange} />
        <Button loading={loading} floated="right" positive type="submit" content="Submit" data-testid="submit-button" />
        <Button onClick={() => navigate(-1)} floated="right" type="button" content="Cancel" data-testid="cancel-button" />
      </Form>
    </Segment>
  );
});
