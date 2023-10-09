"use client";

import Link from "next/link";
import React, { use } from "react";
import { buttonVariants } from "./ui/button";
import useAuth from "@/hooks/useAuth";

function Navbar() {
  const { auth } = useAuth();

  return (
    <nav className='bg-zinc-100 py-2 border-b border-s-zinc-200 fixed w-full z-10 top-0'>
      <div className='container flex item-center justify-between'>
        <Link href='/'>Logo</Link>
        {auth && auth.user && (
          <form
            method='POST'
            action={`${process.env.NEXT_PUBLIC_API_URL}/auth/logout`}
          >
            <button type='submit' className={buttonVariants()}>
              Logout
            </button>
          </form>
        )}

        {!auth && (
          <Link href='/login' className={buttonVariants()}>
            Login
          </Link>
        )}
      </div>
    </nav>
  );
}

export default Navbar;
