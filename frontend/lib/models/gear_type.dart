import 'package:json_annotation/json_annotation.dart';

@JsonEnum()
enum GearType {
  @JsonValue(0)
  jacket,
  @JsonValue(1)
  pants,
  @JsonValue(2)
  fireJacket,
  @JsonValue(3)
  firePants,
  @JsonValue(4)
  helmet,
  @JsonValue(5)
  boots,
  @JsonValue(6)
  gloves,
  @JsonValue(7)
  vest,
  @JsonValue(8)
  fireHood,
  @JsonValue(9)
  belt,
}
