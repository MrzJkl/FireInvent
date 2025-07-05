import 'package:flutter/widgets.dart';
import 'package:json_annotation/json_annotation.dart';

part 'create_clothing_variant_model.g.dart';

@JsonSerializable()
@immutable
class CreateClothingVariantModel {
  final String productId;
  final String name;
  final String? additionalSpecs;

  const CreateClothingVariantModel({
    required this.productId,
    required this.name,
    this.additionalSpecs,
  });

  factory CreateClothingVariantModel.fromJson(Map<String, dynamic> json) =>
      _$CreateClothingVariantModelFromJson(json);
  Map<String, dynamic> toJson() => _$CreateClothingVariantModelToJson(this);
}
