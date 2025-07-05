import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';
import 'create_models/create_clothing_variant_model.dart';

part 'clothing_variant_model.g.dart';

@JsonSerializable()
@immutable
class ClothingVariantModel extends CreateClothingVariantModel {
  final String id;

  const ClothingVariantModel({
    required this.id,
    required super.productId,
    required super.name,
    super.additionalSpecs,
  });

  factory ClothingVariantModel.fromJson(Map<String, dynamic> json) =>
      _$ClothingVariantModelFromJson(json);
  @override
  Map<String, dynamic> toJson() => _$ClothingVariantModelToJson(this);
}
