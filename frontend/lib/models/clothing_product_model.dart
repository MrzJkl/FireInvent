import 'package:json_annotation/json_annotation.dart';

part 'clothing_product_model.g.dart';

enum GearType {
  @JsonValue(0)
  helmet,
  @JsonValue(1)
  jacket,
  @JsonValue(2)
  trousers,
  @JsonValue(3)
  gloves,
  @JsonValue(4)
  boots,
  @JsonValue(5)
  belt,
  @JsonValue(6)
  harness,
  @JsonValue(7)
  coverall,
  @JsonValue(8)
  vest,
  @JsonValue(9)
  other,
}

@JsonSerializable()
class ClothingProductModel {
  final String? id;
  final String name;
  final String manufacturer;
  final String? description;
  final GearType type;

  ClothingProductModel({
    this.id,
    required this.name,
    required this.manufacturer,
    this.description,
    required this.type,
  });

  factory ClothingProductModel.fromJson(Map<String, dynamic> json) => _$ClothingProductModelFromJson(json);
  Map<String, dynamic> toJson() => _$ClothingProductModelToJson(this);
}