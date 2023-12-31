"use client";

import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { EyeClosedIcon, EyeOpenIcon } from "@radix-ui/react-icons";
import { useForm } from "react-hook-form";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "../../ui/form";
import { Input } from "../../ui/input";
import { Button } from "../../ui/button";
import { PUBLIC_API } from "@/lib/api";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { useState } from "react";
import { AbsoluteSpinner } from "../../ui/spinner";
import { PASSWORD_REGEX } from "@/lib/constants";

const RegisterFormSchema = z
  .object({
    email: z.string().email(),
    password: z.string().min(6).max(255).regex(PASSWORD_REGEX),
    confirmPassword: z.string().min(6).max(255).regex(PASSWORD_REGEX),
  })
  .refine((data) => data.password === data.confirmPassword, {
    path: ["confirmPassword"],
    message: "Passwords do not match",
  });

export default function RegisterForm() {
  const { push } = useRouter();

  const [error, setError] = useState<string | null>(null);
  const [passwordVisible, setPasswordVisible] = useState(false);

  const form = useForm<z.infer<typeof RegisterFormSchema>>({
    resolver: zodResolver(RegisterFormSchema),
    mode: "onChange",
    defaultValues: {
      email: "",
      password: "",
      confirmPassword: "",
    },
  });

  async function onSubmit(values: z.infer<typeof RegisterFormSchema>) {
    setError(null);

    const { email, password, confirmPassword } = values;
    if (!email || !password || !confirmPassword) return;

    try {
      await PUBLIC_API.post(
        "/auth/register",
        JSON.stringify({ email, password }),
        {
          headers: {
            "Content-Type": "application/json",
          },
        },
      );

      push("/login");
    } catch (error: any) {
      const { response } = error;
      if (response.status === 400) {
        const { details } = response?.data?.at(0);

        setError(details ?? "Something went wrong");
        return;
      }

      if (response.status !== 200) {
        setError("Something went wrong, please try again");
        return;
      }
    }
  }

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)}>
        {form.formState.isSubmitting && <AbsoluteSpinner />}
        {error && error !== "" && (
          <FormMessage className="pb-2">{error}</FormMessage>
        )}
        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email</FormLabel>
              <FormControl>
                <Input placeholder="Enter your email address" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <FormItem className="my-4">
              <FormLabel>Password</FormLabel>
              <FormControl>
                <div className="relative">
                  <Button
                    variant="ghost"
                    size="icon"
                    type="button"
                    className="absolute right-2"
                    onClick={() => setPasswordVisible(!passwordVisible)}
                  >
                    {passwordVisible ? <EyeOpenIcon /> : <EyeClosedIcon />}
                  </Button>
                  <Input
                    placeholder="Enter your password"
                    type={passwordVisible ? "text" : "password"}
                    {...field}
                  />
                </div>
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="confirmPassword"
          render={({ field }) => (
            <FormItem className="my-4">
              <FormLabel>Confirm password</FormLabel>
              <FormControl>
                <Input
                  placeholder="Confirm your password"
                  type={passwordVisible ? "text" : "password"}
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <Button
          type="submit"
          className="w-full"
          disabled={form.formState.isSubmitting}
        >
          Sign Up
        </Button>
      </form>
      <p>
        Already have an account?{" "}
        <Link href="/login" className="text-blue-500 hover:underline">
          Login
        </Link>
      </p>
    </Form>
  );
}
