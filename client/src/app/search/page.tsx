import React, { Suspense } from "react";

export default function Search({ searchParams }: { searchParams: any }) {
  const { search } = searchParams;

  return (
    <div className="container flex flex-col pt-16">
      <h1>Search</h1>
      <Suspense key={search} fallback={"Loading..."}>
        <div>Search: {search}</div>
      </Suspense>
    </div>
  );
}
