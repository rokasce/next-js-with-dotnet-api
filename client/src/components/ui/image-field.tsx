import React, { ChangeEvent, useEffect, useState } from "react";

import {
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "./form";
import { Input } from "./input";
import { Avatar, AvatarImage } from "./avatar";

type Props = {
  name: string;
  label: string;
  description: string;
  form: any;
};

function getImageData(event: ChangeEvent<HTMLInputElement>) {
  const dataTransfer = new DataTransfer();

  Array.from(event.target.files!).forEach((image) =>
    dataTransfer.items.add(image),
  );

  const files = dataTransfer.files;
  const displayUrl = URL.createObjectURL(event.target.files![0]);

  return { files, displayUrl };
}

export function ImageField({ name, label, description, form }: Props) {
  const [preview, setPreview] = useState<string | undefined>();

  const previewUrl = form.getValues()[name];

  useEffect(() => {
    if (!preview && previewUrl) {
      setPreview(previewUrl);
    }
  }, [preview, previewUrl]);

  return (
    <>
      <Avatar className="h-24 w-24">
        <AvatarImage src={preview} />
      </Avatar>
      <FormField
        control={form.control}
        name={name}
        render={({ field: { onChange, value, ...rest } }) => (
          <>
            <FormItem>
              <FormLabel>{label}</FormLabel>
              <FormControl>
                <Input
                  type="file"
                  {...rest}
                  onChange={(event) => {
                    const { files, displayUrl } = getImageData(event);
                    setPreview(displayUrl);
                    onChange(files);
                  }}
                  accept="image/png, image/gif, image/jpeg"
                />
              </FormControl>
              <FormDescription>{description}</FormDescription>
              <FormMessage />
            </FormItem>
          </>
        )}
      />
    </>
  );
}
