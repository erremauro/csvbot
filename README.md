# csvbot

CSVBot is an utility to move, copy and rename columns in a `*.csv` file by
defining a `FileStrategy` and `Directives`.

## Table of content

- [Configurations](#configurations)
- [Defining a Source](#defining-a-source)
- [File Strategies](#file-strategies)
  - [Accepted overwrite values](#accepted-overwrite-attribute-values)
  - [Moving and copying a group of iles](#moving-and-copying-a-group-of-files)
- [Directive](#directives)
  - [Accepted directive attributes](#accepted-directives-attributes)
- [Separator](#separator)
- [Logs](#logs)

## Configurations

All configurations can be specified in `config.xml` via xml Elements and
attributes inside the `<Config></Config>` root node.

Here's a `config.xml` example:

```xml
<Config>
  <Source path="C:\mycsv" />
  <FileStrategy>
    <Move path="\\151.92.85.30\shared" overwrite="skip" />
    <Copy path="D:\processed" with="txt" />
  </FileStrategy>
  <GroupedWith>txt,tif</GroupedWith>
  <Directives>
    <Directive position="12" action="copy" to="1" rename="Custom ID" />
    <Directive position="3" rename="Image Name" />
    <Directive position="7" action="move" to="4" />
  </Directive>
  <Separator>;</Separator>
</Config>
```

## Defining a source

A source can be a `file` or `directory` and is a required parameter in order for
the application to work. 

When a directory is specified, csvbot  will process all `*.csv` files found in 
that directory by applying all `FileStrategy` and `Directives` to each file.

To specify a source use a `Source` element inside the `Config` root node:

```xml
<Config>
  <Source path="C:\path\to\source" />
</Config>
```

## File Strategies

A `FileStrategy` tells csvbot what to do with the file once all `Directives`
has been applied. If no `FileStrategy` is specified, csvbot will overwrite the
processed file directly in the source.

You can specify how many `FileStrategy` you want; so you can, for example, move
the file to a location and copy it to another.

To specify a `FileStrategy` use a `Move` or a `Copy` element inside a 
`FileStrategy` container in a `Config` root node. Each `FileStrategy` requires a 
path and optional `overwrite` attribute. Additionally you can specify a `with`
attribute to define a set of comma separated extesions that will match the
source filename and, if found, copied along.

```xml
<Config>
  <FileStrategy>
    <Copy path="D:\processed_files" overwrite="false" />
    <Move path="\\my-host\shared" />
  </FileStrategy>
</Config>
```

### Accepted overwrite attribute values

Here's a list of every accepted overwrite attribute values. When an `overwrite` 
attribute is not specified, csvbot will assume the default value of `true`.

| Value | Description                                                        |
|-------|--------------------------------------------------------------------|
| true  | Default value. Overwrites existing files                           |
| false | Do not overwrite existing files and halt the program with an error |
| skip  | Do not overwrite existing files and continue without notice        |

### Moving and Copying a group of files

You can use a `GroupedWith` container or a `FileStrategy`'s `with` attribute to
move or copy a group of additinal files besides your csv.

```xml
<Config>
  <FileStrategy>
    <Copy path="D:\processed_files" />
    <Move path="\\my-host\shared" with="txt,tiff" />
  </FileStrategy>
  <GroupedWith>txt</GroupedWith>
</Config>
```

Csvbot will search the source directory for files with identical name but 
of specified extensions and it will move or copy those files along. Use a
`GroupedWith` container to apply this behavior to every `FileStrategy` or a 
`FileStrategy`'s `with` attribute to apply the behavior to selected strategies.

## Directives

Use `Directives` to instruct csvbot on how to manipulate your `*.csv` files, by
giving the column `position` and an optional `action` of `move` or  `copy` with
related `to` position. You can also specify an optional `rename` attribute to
rename your column.

List your directives as child of a `Directives` element inside the `Config` root
node:

```xml
<Config>
  <Directives>
    <Directive position="12" action="copy" to="1" rename="Custom ID" />
    <Directive position="3" action="move" to="4" />
    <Directive position="7" rename="Image Name" />
  </Directives>
</Config>
```

### Directive attributes

Here's the accepted directive attributes

| Parameter | Type    | Description                                                   |
|-----------|---------|---------------------------------------------------------------|
| position  | integer | Required column position                                      |
| action    | string  | Optional action name. Can be: _move_ or _copy_                |
| to        | integer | Define new position if _move_ or _copy_ action are specified. |
| rename    | string  | Optional column header new name                               |

## Separator

You can specify a custom csv separator in a `Separator` element inside the
`Config` root node:

```xml
<Config>
  <Sperator>;</Separator>
</Config>
```

## Logs

A `log.txt` file is written for debugging purposes in the csvbot directory. 
The log file is automatically rolled when the file size is over 1MB.