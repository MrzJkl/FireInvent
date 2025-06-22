// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'clothing_variant_model.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

ClothingVariantModel _$ClothingVariantModelFromJson(
  Map<String, dynamic> json,
) => ClothingVariantModel(
  id: json['id'] as String?,
  productId: json['productId'] as String,
  name: json['name'] as String,
  additionalSpecs: json['additionalSpecs'] as String?,
);

Map<String, dynamic> _$ClothingVariantModelToJson(
  ClothingVariantModel instance,
) => <String, dynamic>{
  'id': instance.id,
  'productId': instance.productId,
  'name': instance.name,
  'additionalSpecs': instance.additionalSpecs,
};
