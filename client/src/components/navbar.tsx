"use client";

import Link from "next/link";
import useAuth from "@/hooks/useAuth";
import { buttonVariants } from "./ui/button";
import { UserNav } from "./auth/userNav";
import { ModeToggle } from "./modeToggle";

export function NavigationBar() {
  return (
    <nav className="fixed top-0 z-10 w-full border-b border-s-zinc-200 bg-zinc-100 py-2">
      <div className="item-center container flex justify-between">
        <Link href="/">Logo</Link>

        <ModeToggle />

        <Link href="/login" className={buttonVariants()}>
          Login
        </Link>

      </div>
    </nav>
  );
}

export function UserNavigationBar() {
  const { auth } = useAuth();

  if (!auth) {
    return null;
  }

  return(
      <nav className="fixed top-0 z-10 w-full border-b border-s-zinc-200 bg-zinc-100 py-2">
        <div className="item-center container flex justify-between">
          <Link href="/home">Logo</Link>

          <ModeToggle />

          <UserNav email={auth.user.email} />
        </div>
      </nav>
    );
}
