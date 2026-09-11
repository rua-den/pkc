export function WorkPlayDetailPage({ id }: { id: string }) {
  async function handleComplete() {
    await fetch(`/api/workplays/${id}/complete`, { method: "POST" });
  }

  async function handleCancel() {
    await fetch(`/api/workplays/${id}/cancel`, { method: "POST" });
  }

  async function handleReopen() {
    await fetch(`/api/workplays/${id}/reopen`, { method: "POST" });
  }

  return hasPermission("ManageWorkPlay") && (
    <>
      <button onClick={handleComplete}>Complete</button>
      <button onClick={handleCancel}>Cancel</button>
      <button onClick={handleReopen}>Reopen</button>
    </>
  );
}

export function Routes() {
  return <Route path="/workplays/:id" element={<WorkPlayDetailPage />} />;
}
