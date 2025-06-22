import 'package:json_annotation/json_annotation.dart';

part 'clothing_item_model.g.dart';

enum GearCondition {
  @JsonValue(0)
  excellent,
  @JsonValue(1)
  good,
  @JsonValue(2)
  fair,
  @JsonValue(3)
  poor,
  @JsonValue(4)
  retired,
}

@JsonSerializable()
class ClothingItemModel {
  final String? id;
  final String variantId;
  final String? identifier;
  final String? storageLocationId;
  final GearCondition condition;
  final DateTime purchaseDate;
  final DateTime? retirementDate;

  ClothingItemModel({
    this.id,
    required this.variantId,
    this.identifier,
    this.storageLocationId,
    required this.condition,
    required this.purchaseDate,
    this.retirementDate,
  });

  factory ClothingItemModel.fromJson(Map<String, dynamic> json) => _$ClothingItemModelFromJson(json);
  Map<String, dynamic> toJson() => _$ClothingItemModelToJson(this);
} 
