import React, { ChangeEvent, useEffect, useState } from "react";

import {
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "./form";
import { Avatar, AvatarFallback, AvatarImage } from "./avatar";
import { Input } from "./input";

type Props = {
  name: string;
  description: string;
  form: any;
};

// TODO: Figure out how this works
function getImageData(event: ChangeEvent<HTMLInputElement>) {
  // FileList is immutable, so we need to create a new one
  const dataTransfer = new DataTransfer();

  // Add newly uploaded images
  Array.from(event.target.files!).forEach((image) =>
    dataTransfer.items.add(image),
  );

  const files = dataTransfer.files;
  const displayUrl = URL.createObjectURL(event.target.files![0]);

  return { files, displayUrl };
}

export function ImageField({ name, form, description }: Props) {
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
        {!preview && <AvatarFallback>Loading...</AvatarFallback>}
      </Avatar>
      <FormField
        control={form.control}
        name={name}
        render={({ field: { onChange, value, ...rest } }) => (
          <>
            <FormItem>
              <FormLabel>Circle Image</FormLabel>
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
