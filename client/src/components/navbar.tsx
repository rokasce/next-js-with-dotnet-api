"use client";

import Link from "next/link";
import useAuth from "@/hooks/useAuth";
import { buttonVariants } from "./ui/button";
import { UserNav } from "./auth/userNav";
import { ModeToggle } from "./modeToggle";

export default function Navbar() {
  const { auth } = useAuth();

  const isAuthenticated = auth && auth.user;

  return (
    <nav className="fixed top-0 z-10 w-full border-b border-s-zinc-200 bg-zinc-100 py-2">
      <div className="item-center container flex justify-between">
        <Link href={isAuthenticated ? "/home" : "/"}>Logo</Link>

        <ModeToggle />

        {!isAuthenticated && (
          <Link href="/login" className={buttonVariants()}>
            Login
          </Link>
        )}

        {isAuthenticated && <UserNav email={auth.user.email} />}
      </div>
    </nav>
  );
}
