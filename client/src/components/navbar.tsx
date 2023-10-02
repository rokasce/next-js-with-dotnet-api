import Link from "next/link";
import React from "react";
import { buttonVariants } from "./ui/button";

function Navbar() {
  return (
    <nav className='bg-zinc-100 py-2 border-b border-s-zinc-200 fixed w-full z-10 top-0'>
      <div className='container flex item-center justify-between'>
        <Link href='/'>Logo</Link>
        <Link href='/login' className={buttonVariants()}>
          Login
        </Link>
      </div>
    </nav>
  );
}

export default Navbar;
