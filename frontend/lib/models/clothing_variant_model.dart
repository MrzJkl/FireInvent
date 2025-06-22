import 'package:json_annotation/json_annotation.dart';

part 'clothing_variant_model.g.dart';

@JsonSerializable()
class ClothingVariantModel {
  final String? id;
  final String productId;
  final String name;
  final String? additionalSpecs;

  ClothingVariantModel({
    this.id,
    required this.productId,
    required this.name,
    this.additionalSpecs,
  });

  factory ClothingVariantModel.fromJson(Map<String, dynamic> json) => _$ClothingVariantModelFromJson(json);
  Map<String, dynamic> toJson() => _$ClothingVariantModelToJson(this);
}
