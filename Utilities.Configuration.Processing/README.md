# Utilities.Configuration.Processing

A C# library for advanced configuration processing and transformation.

## Features

### Mixins
Before processing:
```yaml
Family:
  LastName: Smith
Member:
  {Family}: true
  FirstName: John
```
After processing:
```yaml
Family:
  LastName: Smith
Member:
  LastName: Smith
  FirstName: John
```

### Interpolation
Before processing:
```yaml
Manager:
  Name: John Doe
Approver:
  Name: {Manager.Name}
```
After processing:
```yaml
Manager:
  Name: John Doe
Approver:
  Name: John Doe
```