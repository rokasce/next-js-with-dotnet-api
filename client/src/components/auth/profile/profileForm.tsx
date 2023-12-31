"use client";

import * as z from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";

import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { toast } from "@/components/ui/use-toast";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { ImageField } from "@/components/ui/image-field";
import { useEffect, useState } from "react";
import { AbsoluteSpinner } from "@/components/ui/spinner";
import useApi from "@/hooks/useApi";

const profileFormSchema = z.object({
  username: z
    .string()
    .min(2, {
      message: "Username must be at least 2 characters.",
    })
    .max(30, {
      message: "Username must not be longer than 30 characters.",
    }),
  bio: z.string().max(160).min(4),
  avatar: z
    .any()
    .refine((files) => files?.length === 1, "Please upload an image."),
});

type ProfileFormValues = z.infer<typeof profileFormSchema>;

export function ProfileForm() {
  const [loading, setLoading] = useState(true);

  const form = useForm<ProfileFormValues>({
    resolver: zodResolver(profileFormSchema),
    defaultValues: {
      username: "",
      bio: "",
    },
    mode: "onChange",
  });

  const { api } = useApi();

  // TODO: Fix this warning
  useEffect(() => {
    getInitialValues();
  }, []);

  async function getInitialValues(): Promise<void> {
    setLoading(true);
    try {
      const result = await api.get("/profile/me");
      const formData = {
        username: result.data.username,
        bio: result.data.bio,
        avatar: result.data.avatar,
      };
      form.reset(formData);
    } catch (error) {
      form.reset({
        username: "",
        bio: "I own a computer.",
      });
      toast({
        title: "Uh oh! Something went wrong.",
        variant: "destructive",
        description: "There was an error loading your profile.",
      });
    } finally {
      setLoading(false);
    }
  }

  async function onSubmit(data: ProfileFormValues) {
    setLoading(true);
    try {
      const formData = new FormData();
      formData.append("username", data.username);
      formData.append("avatar", data.avatar[0]);
      formData.append("bio", data.bio);

      const result = await api.put("/profile/update", formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      });

      const updatedData = {
        username: result.data.username,
        bio: result.data.bio,
        avatar: result.data.avatar,
      };

      form.reset(updatedData);

      toast({
        title: "Profile updated successfully!",
        description: "Your profile has been updated.",
      });
    } catch (error) {
      toast({
        title: "Uh oh! Something went wrong.",
        variant: "destructive",
        description:
          "There was an error updating your profile. Please try again.",
      });
    } finally {
      setLoading(false);
    }
  }

  return (
    <Form {...form}>
      {loading && <AbsoluteSpinner />}
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-8">
        <FormField
          control={form.control}
          name="username"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Username</FormLabel>
              <FormControl>
                <Input placeholder="shadcn" {...field} />
              </FormControl>
              <FormDescription>
                This is your public display name. It can be your real name or a
                pseudonym. You can only change this once every 30 days.
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="bio"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Bio</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Tell us a little bit about yourself"
                  className="resize-none"
                  {...field}
                />
              </FormControl>
              <FormDescription>
                You can <span>@mention</span> other users and organizations to
                link to them.
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <ImageField
          name="avatar"
          label="Avatar"
          description="This image will be displayed on your profile and next to your comments"
          form={form}
        />

        <Button type="submit">Update profile</Button>
      </form>
    </Form>
  );
}
