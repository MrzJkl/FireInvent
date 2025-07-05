import 'package:json_annotation/json_annotation.dart';

@JsonEnum()
enum GearCondition {
  @JsonValue(0)
  new_,
  @JsonValue(1)
  used,
  @JsonValue(2)
  damaged,
  @JsonValue(3)
  destroyed,
  @JsonValue(4)
  lost,
}
