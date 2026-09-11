export function WorkPlayDetailPage({ id }: { id: string }) {
  async function handleComplete() {
    await fetch(`/api/workplays/${id}/complete`, { method: "POST" });
  }

  return hasPermission("ManageWorkPlay") && (
    <button onClick={handleComplete}>Complete</button>
  );
}

export function Routes() {
  return <Route path="/workplays/:id" element={<WorkPlayDetailPage />} />;
}
